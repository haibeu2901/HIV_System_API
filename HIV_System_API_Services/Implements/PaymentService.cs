using HIV_System_API_BOs;
using HIV_System_API_DTOs.PaymentDTO;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepo _paymentRepo;
        private readonly INotificationService _notificationService;
        private readonly HivSystemApiContext _context;

        public PaymentService()
        {
            _paymentRepo = new PaymentRepo();
            _notificationService = new NotificationService();
            _context = new HivSystemApiContext();
        }

        public PaymentService(IPaymentRepo paymentRepo, INotificationService notificationService, HivSystemApiContext context)
        {
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private Payment MapToEntity(PaymentRequestDTO dto, string? paymentIntentId = null)
        {
            return new Payment
            {
                PmrId = dto.PmrId,
                SrvId = dto.SrvId ?? null,
                Amount = dto.Amount,
                Currency = dto.Currency,
                PaymentMethod = dto.PaymentMethod ?? "card",
                Notes = dto.Description,
                PaymentStatus = 1, // Pending
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                PaymentDate = DateTime.Now,
                PaymentIntentId = paymentIntentId
            };
        }

        private async Task<PaymentResponseDTO> MapToResponseDTOAsync(Payment entity)
        {
            // Force fresh data fetch by reloading the entity
            var freshPayment = await _context.Payments
                .Include(p => p.Pmr)
                    .ThenInclude(pmr => pmr.Ptn)
                        .ThenInclude(ptn => ptn.Acc)
                .Include(p => p.Srv)
                .FirstOrDefaultAsync(p => p.PayId == entity.PayId);

            if (freshPayment == null)
                throw new KeyNotFoundException($"Payment with ID {entity.PayId} not found.");

            var patient = freshPayment.Pmr?.Ptn;
            var account = patient?.Acc;
            var service = freshPayment.Srv;

            return new PaymentResponseDTO
            {
                PayId = freshPayment.PayId,
                PmrId = freshPayment.PmrId,
                SrvId = freshPayment.SrvId,
                PaymentIntentId = freshPayment.PaymentIntentId,
                Amount = freshPayment.Amount,
                Currency = freshPayment.Currency,
                PaymentDate = freshPayment.PaymentDate,
                PaymentStatus = freshPayment.PaymentStatus, // This will now reflect the updated status
                PaymentMethod = freshPayment.PaymentMethod,
                Description = freshPayment.Notes,
                CreatedAt = freshPayment.CreatedAt,
                UpdatedAt = freshPayment.UpdatedAt,
                PatientId = patient?.PtnId,
                PatientName = account?.Fullname,
                PatientEmail = account?.Email,
                ServiceName = service?.ServiceName,
                ServicePrice = service?.Price
            };
        }

        public async Task<(string ClientSecret, PaymentResponseDTO Payment)> CreatePaymentWithIntentAsync(PaymentRequestDTO dto)
        {
            // 1. Create Stripe PaymentIntent
            var pmr = await _context.PatientMedicalRecords
                .Include(p => p.Ptn)
                    .ThenInclude(ptn => ptn.Acc)
                .FirstOrDefaultAsync(p => p.PmrId == dto.PmrId);

            var email = pmr?.Ptn?.Acc?.Email;
            if (string.IsNullOrEmpty(email))
                throw new InvalidOperationException("Không tìm thấy email bệnh nhân.");

            var customerService = new CustomerService();
            var customerList = await customerService.ListAsync(new CustomerListOptions { Email = email });
            string customerId;
            if (customerList.Data.Any())
                customerId = customerList.Data.First().Id;
            else
                customerId = (await customerService.CreateAsync(new CustomerCreateOptions { Email = email })).Id;

            long amountInSmallestUnit = (long)dto.Amount;
            if (dto.Currency.ToLower() == "usd")
                amountInSmallestUnit = (long)(dto.Amount * 100);

            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInSmallestUnit,
                Currency = dto.Currency,
                Description = dto.Description,
                PaymentMethodTypes = new List<string> { "card" },
                Customer = customerId,
                ReceiptEmail = email,
                Metadata = new Dictionary<string, string>
                {
                    { "PmrId", dto.PmrId.ToString() },
                    { "SrvId", dto.SrvId.ToString() ?? "" }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            // 2. Create Payment in DB with status Pending and store PaymentIntentId
            var paymentEntity = MapToEntity(dto, paymentIntent.Id);
            var created = await _paymentRepo.CreatePaymentAsync(paymentEntity);
            await SendCardPaymentCreateNotificationAsync(created);
            var paymentDto = await MapToResponseDTOAsync(created);
            return (paymentIntent.ClientSecret, paymentDto);
        }

        public async Task<PaymentResponseDTO?> GetPaymentByIntentIdAsync(string paymentIntentId)
        {
            var all = await _paymentRepo.GetAllPaymentsAsync();
            var entity = all.FirstOrDefault(p => p.PaymentIntentId == paymentIntentId);
            return entity != null ? await MapToResponseDTOAsync(entity) : null;
        }

        public async Task UpdatePaymentStatusByIntentIdAsync(string paymentIntentId, byte status)
        {
            // Get payment by PaymentIntentId using repository
            var allPayments = await _paymentRepo.GetAllPaymentsAsync();
            var payment = allPayments.FirstOrDefault(p => p.PaymentIntentId == paymentIntentId);

            if (payment != null)
            {
                // FIXED: Check if status is already the same to prevent duplicate notifications
                if (payment.PaymentStatus == status)
                {
                    Console.WriteLine($"Payment {payment.PayId} already has status {status}, skipping update and notification");
                    return;
                }

                // Update payment status using repository
                var updatedPayment = await _paymentRepo.UpdatePaymentStatusAsync(payment.PayId, status);

                // Send notification after status update
                await SendPaymentNotificationAsync(updatedPayment, status);
            }
        }

        private async Task SendPaymentNotificationAsync(Payment payment, byte status)
        {
            try
            {
                // Get fresh payment data with navigation properties using repository
                var freshPayment = await _paymentRepo.GetPaymentByIdAsync(payment.PayId);
                if (freshPayment == null) return;

                // For getting account info, we need to use the response DTO mapping
                var paymentDto = await MapToResponseDTOAsync(freshPayment);
                if (string.IsNullOrEmpty(paymentDto.PatientEmail)) return;

                bool isSuccess = (status == 2); // 2 = Success, 3 = Failed
                string serviceName = paymentDto.ServiceName ?? "dịch vụ y tế";
                bool isCashPayment = (freshPayment.PaymentMethod == "cash");
                
                // Create notification message based on payment method
                string notificationMessage;
                string notiType;

                if (isCashPayment)
                {
                    // Cash payment notifications
                    notificationMessage = isSuccess
                        ? $"💰 Thanh toán tiền mặt thành công! Bạn đã thanh toán {freshPayment.Amount:N0} {freshPayment.Currency.ToUpper()} cho {serviceName}. Mã thanh toán: #{freshPayment.PayId}"
                        : $"❌ Thanh toán tiền mặt thất bại! Giao dịch {freshPayment.Amount:N0} {freshPayment.Currency.ToUpper()} cho {serviceName} không thành công. Mã thanh toán: #{freshPayment.PayId}";
                    
                    notiType = isSuccess ? "TT thành công" : "TT thất bại";
                }
                else
                {
                    // Card payment notifications
                    notificationMessage = isSuccess
                        ? $"💳 Thanh toán thẻ thành công! Bạn đã thanh toán {freshPayment.Amount:N0} {freshPayment.Currency.ToUpper()} cho {serviceName}. Mã giao dịch: {freshPayment.PaymentIntentId}"
                        : $"❌ Thanh toán thẻ thất bại! Giao dịch {freshPayment.Amount:N0} {freshPayment.Currency.ToUpper()} cho {serviceName} không thành công. Vui lòng thử lại. Mã giao dịch: {freshPayment.PayId}";
                    
                    notiType = isSuccess ? "TT thành công" : "TT thất bại";
                }

                // Use NotificationService instead of direct repository calls
                var notificationDto = new CreateNotificationRequestDTO
                {
                    NotiType = notiType,
                    NotiMessage = notificationMessage,
                    SendAt = DateTime.Now
                };

                // Get account ID from payment DTO and send notification
                if (paymentDto.PatientId.HasValue)
                {
                    // Find account ID by patient ID
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PtnId == paymentDto.PatientId.Value);
                    if (patient != null)
                    {
                        await _notificationService.CreateAndSendToAccountIdAsync(notificationDto, patient.AccId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending payment notification: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        public async Task<List<PaymentResponseDTO>> GetAllPaymentsAsync()
        {
            var entities = await _paymentRepo.GetAllPaymentsAsync();
            var result = new List<PaymentResponseDTO>();
            
            foreach (var entity in entities)
            {
                result.Add(await MapToResponseDTOAsync(entity));
            }
            
            return result;
        }

        public async Task<PaymentResponseDTO> GetPaymentByIdAsync(int id)
        {
            var entity = await _paymentRepo.GetPaymentByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Thanh toán với ID {id} không tìm thấy.");
            return await MapToResponseDTOAsync(entity);
        }

        public async Task<List<PaymentResponseDTO>> GetPaymentsByPmrIdAsync(int pmrId)
        {
            var entities = await _paymentRepo.GetPaymentsByPatientMedicalRecordIdAsync(pmrId);
            var result = new List<PaymentResponseDTO>();
            
            foreach (var entity in entities)
            {
                result.Add(await MapToResponseDTOAsync(entity));
            }
            
            return result;
        }

        public async Task<PaymentResponseDTO> UpdatePaymentAsync(int payId, PaymentRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var entity = MapToEntity(dto);
            var updated = await _paymentRepo.UpdatePaymentAsync(payId, entity);
            return await MapToResponseDTOAsync(updated);
        }

        public async Task<bool> DeletePaymentAsync(int payId)
        {
            return await _paymentRepo.DeletePaymentAsync(payId);
        }

        public async Task<List<PaymentResponseDTO>> GetAllPersonalPaymentsAsync(int accId)
        {
            // Find the patient by account ID
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.AccId == accId);
            if (patient == null)
                throw new InvalidOperationException($"No patient found for account ID {accId}.");

            var entities = await _paymentRepo.GetPersonalPaymentsAsync(patient.PtnId);
            var result = new List<PaymentResponseDTO>();
            
            foreach (var entity in entities)
            {
                result.Add(await MapToResponseDTOAsync(entity));
            }
            
            return result;
        }

        public async Task<PaymentResponseDTO> UpdatePaymentStatusByIdAsync(int paymentId, byte status)
        {
            // Query the payment directly by PaymentId
            var payment = await _context.Payments
                .Include(p => p.Pmr)
                    .ThenInclude(pmr => pmr.Ptn)
                        .ThenInclude(ptn => ptn.Acc)
                .Include(p => p.Srv)
                .FirstOrDefaultAsync(p => p.PayId == paymentId);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");

            // FIXED: Store old status for comparison
            var oldStatus = payment.PaymentStatus;
            
            // Update payment status and method for cash payments
            payment.PaymentStatus = status;
            payment.UpdatedAt = DateTime.Now;
            
            // If marking as successful and it's not a card payment, set as cash
            if (status == 2 && (payment.PaymentMethod == null || payment.PaymentMethod == "card"))
            {
                payment.PaymentMethod = "cash";
            }

            await _context.SaveChangesAsync();

            // FIXED: Send notification only if status actually changed
            if (oldStatus != status)
            {
                await SendPaymentNotificationAsync(payment, status);
            }
            else
            {
                Console.WriteLine($"Payment {paymentId} status unchanged ({status}), notification not sent");
            }

            return await MapToResponseDTOAsync(payment);
        }

        public async Task<PaymentResponseDTO> CreateCashPaymentAsync(CashPaymentRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than 0", nameof(dto.Amount));

            if (string.IsNullOrWhiteSpace(dto.Currency))
                throw new ArgumentException("Currency is required", nameof(dto.Currency));

            // Validate patient medical record exists
            var pmr = await _context.PatientMedicalRecords
                .Include(p => p.Ptn)
                    .ThenInclude(ptn => ptn.Acc)
                .FirstOrDefaultAsync(p => p.PmrId == dto.PmrId);

            if (pmr == null)
                throw new InvalidOperationException($"Patient medical record with ID {dto.PmrId} not found.");

            // Validate service exists if provided
            if (dto.SrvId.HasValue)
            {
                var serviceExists = await _context.MedicalServices.AnyAsync(s => s.SrvId == dto.SrvId.Value);
                if (!serviceExists)
                    throw new InvalidOperationException($"Medical service with ID {dto.SrvId} not found.");
            }

            // Create cash payment entity (NO STRIPE INTERACTION)
            var cashPayment = new Payment
            {
                PmrId = dto.PmrId,
                SrvId = dto.SrvId,
                Amount = dto.Amount,
                Currency = dto.Currency.ToLower(),
                PaymentMethod = "cash",
                Notes = !string.IsNullOrEmpty(dto.Notes) ? dto.Notes : dto.Description,
                PaymentStatus = 1, // Pending - waiting for staff confirmation
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                PaymentDate = DateTime.Now,
                PaymentIntentId = null // NO Stripe PaymentIntent for cash payments
            };

            // Save to database
            var created = await _paymentRepo.CreatePaymentAsync(cashPayment);

            // Send notification to patient about cash payment creation
            await SendCashPaymentCreatedNotificationAsync(created);

            return await MapToResponseDTOAsync(created);
        }

        private async Task SendCashPaymentCreatedNotificationAsync(Payment payment)
        {
            try
            {
                // Get payment with full details using the response DTO mapping
                var paymentDto = await MapToResponseDTOAsync(payment);
                if (string.IsNullOrEmpty(paymentDto.PatientEmail)) return;

                var serviceName = paymentDto.ServiceName ?? "dịch vụ y tế";
                
                var notificationMessage = $"💰 Yêu cầu thanh toán tiền mặt đã được tạo! Số tiền: {payment.Amount:N0} {payment.Currency.ToUpper()} cho {serviceName}. Vui lòng thanh toán tại quầy lễ tân. Mã thanh toán: #{payment.PayId}";

                var notificationDto = new CreateNotificationRequestDTO
                {
                    NotiType = "TT tiền mặt",
                    NotiMessage = notificationMessage,
                    SendAt = DateTime.Now
                };

                // Get account ID from payment DTO and send notification
                if (paymentDto.PatientId.HasValue)
                {
                    // Find account ID by patient ID
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PtnId == paymentDto.PatientId.Value);
                    if (patient != null)
                    {
                        await _notificationService.CreateAndSendToAccountIdAsync(notificationDto, patient.AccId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending cash payment notification: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        private async Task SendCardPaymentCreateNotificationAsync(Payment payment)
        {
            try
            {
                // Get payment with full details using the response DTO mapping
                var paymentDto = await MapToResponseDTOAsync(payment);
                if (string.IsNullOrEmpty(paymentDto.PatientEmail)) return;

                var serviceName = paymentDto.ServiceName ?? "dịch vụ y tế";
                
                var notificationMessage = $"💳 Yêu cầu thanh toán thẻ đã được tạo! Số tiền: {payment.Amount:N0} {payment.Currency.ToUpper()} cho {serviceName}. Mã giao dịch: {payment.PayId}";
                
                var notificationDto = new CreateNotificationRequestDTO
                {
                    NotiType = "TT thẻ",
                    NotiMessage = notificationMessage,
                    SendAt = DateTime.Now
                };

                // Get account ID from payment DTO and send notification
                if (paymentDto.PatientId.HasValue)
                {
                    // Find account ID by patient ID
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PtnId == paymentDto.PatientId.Value);
                    if (patient != null)
                    {
                        await _notificationService.CreateAndSendToAccountIdAsync(notificationDto, patient.AccId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending card payment notification: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }
    }
}
