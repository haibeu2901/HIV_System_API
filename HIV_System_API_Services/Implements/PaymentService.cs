using HIV_System_API_BOs;
using HIV_System_API_DTOs.PaymentDTO;
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
        private readonly HivSystemApiContext _context;

        public PaymentService()
        {
            _paymentRepo = new PaymentRepo();
            _context = new HivSystemApiContext();
        }

        public PaymentService(IPaymentRepo paymentRepo, HivSystemApiContext context)
        {
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
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
                PaymentStatus = 0, // Pending
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow,
                PaymentIntentId = paymentIntentId
            };
        }

        private PaymentResponseDTO MapToResponseDTO(Payment entity)
        {
            var pmr = _context.PatientMedicalRecords
                .Include(p => p.Ptn)
                    .ThenInclude(ptn => ptn.Acc)
                .FirstOrDefault(p => p.PmrId == entity.PmrId);

            var patient = pmr?.Ptn;
            var account = patient?.Acc;

            var service = entity.SrvId.HasValue
                ? _context.MedicalServices.FirstOrDefault(s => s.SrvId == entity.SrvId.Value)
                : null;

            return new PaymentResponseDTO
            {
                PayId = entity.PayId,
                PmrId = entity.PmrId,
                SrvId = entity.SrvId ?? null,
                PaymentIntentId = entity.PaymentIntentId,
                Amount = entity.Amount,
                Currency = entity.Currency,
                PaymentDate = entity.PaymentDate,
                PaymentStatus = entity.PaymentStatus,
                PaymentMethod = entity.PaymentMethod,
                Description = entity.Notes,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt,
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
                throw new InvalidOperationException("Patient email not found.");

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

            var paymentDto = MapToResponseDTO(created);
            return (paymentIntent.ClientSecret, paymentDto);
        }

        public async Task<PaymentResponseDTO?> GetPaymentByIntentIdAsync(string paymentIntentId)
        {
            var all = await _paymentRepo.GetAllPaymentsAsync();
            var entity = all.FirstOrDefault(p => p.PaymentIntentId == paymentIntentId);
            return entity != null ? MapToResponseDTO(entity) : null;
        }

        public async Task UpdatePaymentStatusByIntentIdAsync(string paymentIntentId, byte status)
        {
            // Query the payment directly by PaymentIntentId
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);
            if (payment != null)
            {
                payment.PaymentStatus = status;
                payment.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<PaymentResponseDTO>> GetAllPaymentsAsync()
        {
            var entities = await _paymentRepo.GetAllPaymentsAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<PaymentResponseDTO> GetPaymentByIdAsync(int id)
        {
            var entity = await _paymentRepo.GetPaymentByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");
            return MapToResponseDTO(entity);
        }

        public async Task<List<PaymentResponseDTO>> GetPaymentsByPmrIdAsync(int pmrId)
        {
            var entities = await _paymentRepo.GetPaymentsByPatientMedicalRecordIdAsync(pmrId);
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<PaymentResponseDTO> UpdatePaymentAsync(int payId, PaymentRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var entity = MapToEntity(dto);
            var updated = await _paymentRepo.UpdatePaymentAsync(payId, entity);
            return MapToResponseDTO(updated);
        }

        public async Task<bool> DeletePaymentAsync(int payId)
        {
            return await _paymentRepo.DeletePaymentAsync(payId);
        }
    }
}
