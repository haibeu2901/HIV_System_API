using Azure.Core;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.AppointmentDTO;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepo _appointmentRepo;
        private readonly INotificationRepo _notificationRepo;
        private readonly HivSystemApiContext _context;

        public AppointmentService()
        {
            _appointmentRepo = new AppointmentRepo();
            _notificationRepo = new NotificationRepo();
            _context = new HivSystemApiContext();
        }

        private Appointment MapToEntity(AppointmentRequestDTO requestDTO)
        {
            return new Appointment
            {
                PtnId = requestDTO.PatientId,
                DctId = requestDTO.DoctorId,
                ApmtDate = requestDTO.ApmtDate,
                ApmTime = requestDTO.ApmTime,
                Notes = requestDTO.Notes,
                ApmStatus = requestDTO.ApmStatus
            };
        }

        private AppointmentResponseDTO MapToResponseDTO(Appointment appointment)
        {
            // Fetch patient name
            var patient = _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefault(p => p.PtnId == appointment.PtnId)
                ?? throw new InvalidOperationException("Không tìm thấy bệnh nhân liên quan.");

            // Fetch doctor name
            var doctor = _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefault(d => d.DctId == appointment.DctId)
                ?? throw new InvalidOperationException("Không tìm thấy bác sĩ liên quan.");

            return new AppointmentResponseDTO
            {
                AppointmentId = appointment.ApmId,
                PatientId = appointment.PtnId,
                PatientName = patient.Acc.Fullname,
                DoctorId = appointment.DctId,
                DoctorName = doctor.Acc.Fullname,
                ApmtDate = appointment.ApmtDate,
                ApmTime = appointment.ApmTime,
                Notes = appointment.Notes,
                ApmStatus = appointment.ApmStatus,
                RequestDate = appointment.RequestDate,
                RequestTime = appointment.RequestTime,
                RequestBy = appointment.RequestBy
            };
        }

        private string GetAppointmentDateTimeText(Appointment appointment)
        {
            if (appointment.ApmtDate.HasValue && appointment.ApmTime.HasValue)
            {
                return $"vào {appointment.ApmtDate.Value:yyyy-MM-dd} lúc {appointment.ApmTime.Value:HH:mm}";
            }
            else if (appointment.RequestDate.HasValue && appointment.RequestTime.HasValue)
            {
                return $"vào {appointment.RequestDate.Value:yyyy-MM-dd} lúc {appointment.RequestTime.Value:HH:mm}";
            }
            else 
            {
                return "(chưa có lịch cụ thể)";
            }
        }

        private string GetRequestDateTimeText(DateOnly requestDate, TimeOnly requestTime)
        {
            return $"vào {requestDate:yyyy-MM-dd} lúc {requestTime:HH:mm}";
        }

        private async Task ValidateAppointmentAsync(AppointmentRequestDTO request, bool validateStatus, int? apmId = null)
        {
            // Validate PatientId
            if (!await _context.Patients.AnyAsync(p => p.PtnId == request.PatientId))
                throw new ArgumentException("Bệnh nhân không tồn tại.");

            // Validate DoctorId
            if (!await _context.Doctors.AnyAsync(d => d.DctId == request.DoctorId))
                throw new ArgumentException("Bác sĩ không tồn tại.");

            // Validate ApmStatus (only for updates)
            if (validateStatus && (request.ApmStatus < 1 || request.ApmStatus > 5))
                throw new ArgumentException("Trạng thái cuộc hẹn không hợp lệ. Phải nằm trong khoảng từ 1 đến 5.");

            // Only validate date/time constraints if both ApmtDate and ApmTime are provided
            // This allows for appointment requests with null scheduled dates/times
            if (request.ApmtDate != default(DateOnly) && request.ApmTime != default(TimeOnly))
            {
                // Validate ApmtDate and ApmTime
                var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
                var nowTime = TimeOnly.FromDateTime(DateTime.UtcNow);

                if (request.ApmtDate < todayDate)
                    throw new ArgumentException("Ngày hẹn không được ở trong quá khứ.");
                if (request.ApmtDate == todayDate && request.ApmTime < nowTime)
                    throw new ArgumentException("Thời gian hẹn không được ở trong quá khứ đối với ngày hôm nay.");

                var dayOfWeek = (byte)request.ApmtDate.ToDateTime(TimeOnly.MinValue).DayOfWeek;

                // Get all available schedules for the doctor on the requested day and date
                var schedules = await _context.DoctorWorkSchedules
                    .Where(s => s.DoctorId == request.DoctorId && s.IsAvailable && s.WorkDate == request.ApmtDate)
                    .ToListAsync();

                if (schedules == null || schedules.Count == 0)
                {
                    // If no schedule for the requested date, suggest the nearest available date
                    var today = request.ApmtDate;
                    var maxSearchDays = 30;
                    DateOnly? nearestDate = null;
                    for (int i = 1; i <= maxSearchDays; i++)
                    {
                        var nextDate = today.AddDays(i);
                        var hasSchedule = await _context.DoctorWorkSchedules
                            .AnyAsync(s => s.DoctorId == request.DoctorId && s.IsAvailable && s.WorkDate == nextDate);
                        if (hasSchedule)
                        {
                            nearestDate = nextDate;
                            break;
                        }
                    }
                    var message = "Bác sĩ không có lịch làm việc vào ngày này.";
                    if (nearestDate.HasValue)
                        message += $" Ngày khả dụng gần nhất: {nearestDate.Value:yyyy-MM-dd}.";
                    throw new InvalidOperationException(message);
                }

                // Check if requested time is within any available schedule for the date
                var isWithinSchedule = schedules.Any(s =>
                    request.ApmTime >= s.StartTime &&
                    request.ApmTime < s.EndTime
                );

                if (!isWithinSchedule)
                {
                    // Suggest all available time slots for this date
                    var availableSlots = schedules
                        .Select(s => $"{s.StartTime:HH\\:mm} - {s.EndTime:HH\\:mm}")
                        .ToList();

                    var slotsMessage = availableSlots.Count > 0
                        ? string.Join(", ", availableSlots)
                        : "Không có khung giờ khả dụng.";

                    throw new InvalidOperationException(
                        $"Thời gian hẹn yêu cầu nằm ngoài lịch làm việc khả dụng của bác sĩ. " +
                        $"Khung giờ làm việc của bác sĩ vào ngày {request.ApmtDate:yyyy-MM-dd}: {slotsMessage}."
                    );
                }

                // Assume each appointment is 30 minutes
                var appointmentDuration = TimeSpan.FromMinutes(30);
                var apmTime = request.ApmTime;
                var apmEnd = apmTime.Add(appointmentDuration);

                // Fetch all appointments for the doctor on the date (not cancelled)
                // Only check appointments that have actual scheduled dates/times (not null)
                var appointmentsOnDate = await _context.Appointments
                    .Where(a => a.DctId == request.DoctorId
                        && a.ApmtDate == request.ApmtDate
                        && a.ApmtDate.HasValue
                        && a.ApmTime.HasValue
                        && a.ApmStatus != 4) // 4 = Cancelled
                    .ToListAsync();

                // Check for overlapping appointments (not cancelled)
                var hasOverlap = appointmentsOnDate.Any(a =>
                {
                    if (!a.ApmTime.HasValue) return false; // Skip appointments without scheduled time

                    var existingStart = a.ApmTime.Value;
                    var existingEnd = a.ApmTime.Value.Add(appointmentDuration);
                    return existingStart < apmEnd && existingEnd > apmTime
                        && (!apmId.HasValue || a.ApmId != apmId.Value);
                });

                if (hasOverlap)
                {
                    throw new InvalidOperationException(
                        $"Bác sĩ có một cuộc hẹn trùng lặp vào thời điểm này."
                    );
                }
            }
        }

        public async Task<AppointmentResponseDTO> ChangeAppointmentStatusAsync(int id, byte status, int accId)
        {
            Debug.WriteLine($"Changing status of appointment with ApmId: {id} to {status}");
            
            if (status < 1 || status > 5)
                throw new ArgumentException("Trạng thái cuộc hẹn không hợp lệ. Phải nằm trong khoảng từ 1 đến 5.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Load appointment with necessary related data in a single query
                var appointment = await _context.Appointments
                    .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                    .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                    .FirstOrDefaultAsync(a => a.ApmId == id);

                if (appointment == null)
                    throw new InvalidOperationException($"Không tìm thấy cuộc hẹn với ID {id}.");

                // Check time constraints if appointment has scheduled date/time
                if (appointment.ApmtDate.HasValue && appointment.ApmTime.HasValue)
                {
                    var appointmentDateTime = appointment.ApmtDate.Value.ToDateTime(appointment.ApmTime.Value);
                    var timeDifference = appointmentDateTime - DateTime.UtcNow;

                    if (timeDifference.TotalHours < 12)
                        throw new InvalidOperationException("Không thể thay đổi trạng thái cuộc hẹn vì thời gian hẹn nhỏ hơn 12 giờ từ bây giờ.");
                }

                // Only doctor can change status to 5 (Completed)
                if (status == 5 && appointment.DctId != accId)
                    throw new UnauthorizedAccessException("Chỉ bác sĩ của cuộc hẹn này mới có thể hoàn thành nó.");

                // If confirming appointment (status 2 or 3), copy request date/time to actual appointment date/time
                if ((status == 2 || status == 3) && appointment.RequestDate.HasValue && appointment.RequestTime.HasValue)
                {
                    // Check if the user trying to change status is the same as the one who requested the appointment
                    // Can not accept or confirm an appointment that the user created
                    if (appointment.RequestBy == accId)
                        throw new InvalidOperationException("Bạn không thể thay đổi trạng thái cuộc hẹn mà chính bạn đã tạo.");

                    appointment.ApmtDate = appointment.RequestDate;
                    appointment.ApmTime = appointment.RequestTime;

                    // Prepare DTO for validation
                    var requestDto = new AppointmentRequestDTO
                    {
                        PatientId = appointment.PtnId,
                        DoctorId = appointment.DctId,
                        ApmtDate = appointment.RequestDate.Value,
                        ApmTime = appointment.RequestTime.Value,
                        Notes = appointment.Notes,
                        ApmStatus = status
                    };

                    // Validate the appointment
                    await ValidateAppointmentAsync(requestDto, validateStatus: true, apmId: appointment.ApmId);
                }

                // Update appointment status
                appointment.ApmStatus = status;
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                // Create notification
                var notification = new Notification
                {
                    NotiType = GetNotificationTypeForStatus(status),
                    NotiMessage = CreateNotificationMessage(appointment, status),
                    SendAt = DateTime.UtcNow
                };

                // Save notification and create notification accounts in a single operation
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Create notification accounts for both patient and doctor
                var notificationAccounts = new[]
                {
                    new NotificationAccount { NtfId = notification.NtfId, AccId = appointment.PtnId },
                    new NotificationAccount { NtfId = notification.NtfId, AccId = appointment.DctId }
                };

                _context.NotificationAccounts.AddRange(notificationAccounts);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return MapToResponseDTO(appointment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to change appointment status: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        private string GetNotificationTypeForStatus(byte status) => status switch
        {
            2 or 3 => "Xác nhận lịch hẹn",
            4 => "Hủy lịch hẹn",
            5 => "Hoàn tất lịch hẹn",
            _ => "Cập nhật lịch hẹn"
        };

        private string CreateNotificationMessage(Appointment appointment, byte status)
        {
            var doctorName = appointment.Dct?.Acc?.Fullname ?? "Unknown Doctor";
            var patientName = appointment.Ptn?.Acc?.Fullname ?? "Unknown Patient";
            var dateTimeText = GetAppointmentDateTimeText(appointment);

            return status switch
            {
                2 or 3 => $"Cuộc hẹn của bạn {dateTimeText} giữa bệnh nhân {patientName} với bác sĩ {doctorName} đã được xác nhận.",
                4 => $"Cuộc hẹn của bạn {dateTimeText} giữa bệnh nhân {patientName} với bác sĩ {doctorName} đã bị huỷ.",
                5 => $"Cuộc hẹn của bạn {dateTimeText} giữa bệnh nhân {patientName} với bác sĩ {doctorName} đã thành công.",
                _ => $"Cuộc hẹn của bạn {dateTimeText} giữa bệnh nhân {patientName} với bác sĩ {doctorName} đã được cập nhật."
            };
        }

        public async Task<List<AppointmentResponseDTO>> GetAppointmentsByAccountIdAsync(int accountId, byte role)
        {
            Debug.WriteLine($"Retrieving appointments for account ID: {accountId} with role: {role}");

            try
            {
                var appointments = await _appointmentRepo.GetAppointmentsByAccountIdAsync(accountId, role);
                return appointments.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving appointments: {ex.Message}");
                throw;
            }
        }

        public async Task<AppointmentResponseDTO> CreateAppointmentAsync(CreateAppointmentRequestDTO request, int accId)
        {
            Debug.WriteLine($"Creating appointment for PatientId: {accId}, DoctorId: {request.DoctorId}");
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu DTO là bắt buộc.");

            var patient = await _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefaultAsync(p => p.PtnId == accId);
            if (patient == null)
                throw new ArgumentException("Bệnh nhân không tồn tại.");

            var doctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == request.DoctorId);
            if (doctor == null)
                throw new ArgumentException("Bác sĩ không tồn tại.");

            var appointmentRequestDto = new AppointmentRequestDTO
            {
                PatientId = accId,
                DoctorId = request.DoctorId,
                ApmtDate = request.AppointmentDate,
                ApmTime = request.AppointmentTime,
                Notes = request.Notes,
                ApmStatus = 1
            };
            await ValidateAppointmentAsync(appointmentRequestDto, validateStatus: false);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var createdAppointment = await _appointmentRepo.CreateAppointmentAsync(request, accId);
                var appointmentDto = MapToResponseDTO(createdAppointment);
                var doctorName = appointmentDto.DoctorName;
                var patientName = appointmentDto.PatientName;

                var notification = new Notification
                {
                    NotiType = "Yêu cầu lịch hẹn",
                    NotiMessage = $"Cuộc hẹn của bạn vào {createdAppointment.RequestDate:yyyy-MM-dd} lúc {createdAppointment.RequestTime:HH:mm} giữa bệnh nhân {patientName} với bác sĩ {doctorName} đã được yêu cầu.",
                    SendAt = DateTime.UtcNow
                };
                var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);
                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, createdAppointment.PtnId);
                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, createdAppointment.DctId);

                await transaction.CommitAsync();
                return appointmentDto;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create appointment: {ex.InnerException?.Message ?? ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAppointmentByIdAsync(int id)
        {
            Debug.WriteLine($"Deleting appointment with AppointmentId: {id}");
            return await _appointmentRepo.DeleteAppointmentByIdAsync(id);
        }

        public async Task<List<AppointmentResponseDTO>> GetAllAppointmentsAsync()
        {
            var appointments = await _appointmentRepo.GetAllAppointmentsAsync();
            return appointments.Select(MapToResponseDTO).ToList();
        }

        public async Task<AppointmentResponseDTO?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepo.GetAppointmentByIdAsync(id);
            return appointment != null ? MapToResponseDTO(appointment) : null;
        }

        public async Task<AppointmentResponseDTO> UpdateAppointmentByIdAsync(int id, AppointmentRequestDTO request)
        {
            Debug.WriteLine($"Updating appointment with ApmId: {id}");

            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu cập nhật cuộc hẹn không được để trống.");

            // Check if the appointment exists
            if (!await _context.Appointments.AnyAsync(a => a.ApmId == id))
                throw new KeyNotFoundException($"Không tìm thấy cuộc hẹn với ID {id}.");

            // Validate the appointment details (throws ArgumentException/InvalidOperationException as needed)
            await ValidateAppointmentAsync(request, validateStatus: true, apmId: id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = MapToEntity(request);
                appointment.ApmId = id;

                var updatedAppointment = await _appointmentRepo.UpdateAppointmentByIdAsync(id, appointment);

                if (updatedAppointment == null)
                    throw new InvalidOperationException($"Không thể cập nhật cuộc hẹn với ID {id} do lỗi kho lưu trữ.");

                await transaction.CommitAsync();
                return MapToResponseDTO(updatedAppointment);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Cuộc hẹn đã được sửa đổi bởi người dùng khác. Vui lòng tải lại và thử lại.");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Đã xảy ra lỗi cơ sở dữ liệu khi cập nhật cuộc hẹn: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Đã xảy ra lỗi bất ngờ khi cập nhật cuộc hẹn: {ex.Message}");
            }
        }

        public async Task<AppointmentResponseDTO> UpdateAppointmentRequestAsync(int appointmentId, UpdateAppointmentRequestDTO appointment, int accId)
        {
            Debug.WriteLine($"Updating appointment for AccountId: {accId}");
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Yêu cầu DTO là bắt buộc.");

            // Find the appointment by ID with related entities
            var existingAppointment = await _context.Appointments
                .Include(a => a.Dct)
                    .ThenInclude(d => d.Acc)
                .Include(a => a.Ptn)
                    .ThenInclude(p => p.Acc)
                .FirstOrDefaultAsync(a => a.ApmId == appointmentId);

            if (existingAppointment == null)
                throw new InvalidOperationException("Không tìm thấy cuộc hẹn để cập nhật.");

            // Get the original appointment date/time text before making changes
            string originalDateTimeText = GetAppointmentDateTimeText(existingAppointment);

            // Prepare DTO for validation
            var requestDto = new AppointmentRequestDTO
            {
                PatientId = existingAppointment.PtnId,
                DoctorId = existingAppointment.DctId,
                ApmtDate = appointment.AppointmentDate,
                ApmTime = appointment.AppointmentTime,
                Notes = appointment.Notes,
                ApmStatus = 1 // Back to pending status
            };

            await ValidateAppointmentAsync(requestDto, validateStatus: true, apmId: existingAppointment.ApmId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update fields - these are now request changes, not confirmed appointments
                existingAppointment.RequestDate = appointment.AppointmentDate;
                existingAppointment.RequestTime = appointment.AppointmentTime;
                existingAppointment.RequestBy = accId;
                existingAppointment.Notes = appointment.Notes;

                var updatedAppointment = await _appointmentRepo.UpdateAppointmentAsync(appointmentId, appointment, accId);

                var doctorName = existingAppointment.Dct?.Acc?.Fullname ?? "Unknown Doctor";
                var patientName = existingAppointment.Ptn?.Acc?.Fullname ?? "Unknown Patient";

                // Create notification with original and requested date/time
                string requestedDateTimeText = GetRequestDateTimeText(appointment.AppointmentDate, appointment.AppointmentTime);
                var notificationMessage = $"Có yêu cầu thay đổi lịch hẹn từ {originalDateTimeText} sang {requestedDateTimeText} giữa bệnh nhân {patientName} với bác sĩ {doctorName}.";

                var notification = new Notification
                {
                    NotiType = "Yêu cầu đổi lịch",
                    NotiMessage = notificationMessage,
                    SendAt = DateTime.UtcNow,
                };

                var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

                var account = await _context.Accounts.FindAsync(accId);
                if (account != null && account.Roles == 2) // 2 = Doctor
                {
                    // Send notification to the patient
                    var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PtnId == updatedAppointment.PtnId);
                    if (patient != null)
                        await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, patient.AccId);
                }
                else if (account != null && account.Roles == 3) // 3 = Patient
                {
                    // Send notification to the doctor
                    var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DctId == updatedAppointment.DctId);
                    if (doctor != null)
                        await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, doctor.AccId);
                }

                await transaction.CommitAsync();
                return MapToResponseDTO(updatedAppointment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to update appointment: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<AppointmentResponseDTO>> GetAllPersonalAppointmentsAsync(int accId)
        {
            Debug.WriteLine($"Retrieving all personal appointments for AccountId: {accId}");

            // Validate account existence
            var account = await _context.Accounts.FindAsync(accId);
            if (account == null)
                throw new ArgumentException($"Không tìm thấy tài khoản với ID {accId}.");

            // Optionally, check if account is a patient or doctor
            var isPatient = await _context.Patients.AnyAsync(p => p.PtnId == accId);
            var isDoctor = await _context.Doctors.AnyAsync(d => d.DctId == accId);
            if (!isPatient && !isDoctor)
                throw new InvalidOperationException("Tài khoản không phải là bệnh nhân hoặc bác sĩ.");

            var appointments = await _appointmentRepo.GetAllPersonalAppointmentsAsync(accId);
            return appointments.Select(MapToResponseDTO).ToList();
        }

        public async Task<AppointmentResponseDTO> CompleteAppointmentAsync(int appointmentId, CompleteAppointmentDTO dto, int accId)
        {
            Debug.WriteLine($"Completing appointment with ApmId: {appointmentId} by AccountId: {accId}");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Find the appointment
                var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.ApmId == appointmentId);
                if (appointment == null)
                    throw new InvalidOperationException($"Không tìm thấy cuộc hẹn với ID {appointmentId}.");

                // Only allow doctor to complete
                var account = await _context.Accounts.FindAsync(accId);
                if (account == null)
                    throw new ArgumentException($"Không tìm thấy tài khoản với ID {accId}.");
                    
                var isDoctor = await _context.Doctors.AnyAsync(d => d.AccId == accId && d.DctId == appointment.DctId);
                if (!isDoctor)
                    throw new UnauthorizedAccessException("Chỉ bác sĩ của cuộc hẹn này mới có thể hoàn thành nó.");

                // Only allow if not already completed/cancelled
                if (appointment.ApmStatus == 4)
                    throw new InvalidOperationException("Không thể hoàn thành một cuộc hẹn đã bị hủy.");
                if (appointment.ApmStatus == 5)
                    throw new InvalidOperationException("Cuộc hẹn đã được hoàn thành.");

                // Update notes if provided
                if (!string.IsNullOrWhiteSpace(dto.Notes))
                {
                    appointment.Notes = dto.Notes;
                    _context.Appointments.Update(appointment);
                    await _context.SaveChangesAsync();
                }

                // Ensure patient medical record exists before completing
                var patientMedicalRecord = await _context.PatientMedicalRecords
                    .FirstOrDefaultAsync(r => r.PtnId == appointment.PtnId);
                if (patientMedicalRecord == null)
                {
                    patientMedicalRecord = new PatientMedicalRecord
                    {
                        PtnId = appointment.PtnId,
                    };
                    _context.PatientMedicalRecords.Add(patientMedicalRecord);
                    await _context.SaveChangesAsync();
                }

                // Commit current transaction before calling ChangeAppointmentStatusAsync
                await transaction.CommitAsync();

                // Use ChangeAppointmentStatusAsync to handle status change and notifications
                // This will create its own transaction internally
                var result = await ChangeAppointmentStatusAsync(appointmentId, 5, accId); // 5 = Completed

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to complete appointment: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<AppointmentResponseDTO> GetPersonalAppointmentByIdAsync(int accId, int appointmentId)
        {
            Debug.WriteLine($"Retrieving personal appointment with ID: {appointmentId} for AccountId: {accId}");

            try
            {
                // 1. Validate account existence
                var account = await _context.Accounts.FindAsync(accId);
                if (account == null)
                {
                    throw new ArgumentException($"Tài khoản với {accId} không tồn tại.");
                }

                // 2. Retrieve the appointment
                var appointment = await _appointmentRepo.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Cuộc hẹn với ID {appointmentId} không tìm thấy.");
                }

                // 3. Authorize the user
                // Check if the account belongs to the patient or the doctor of the appointment
                bool isPatientOfAppointment = await _context.Patients.AnyAsync(p => p.AccId == accId && p.PtnId == appointment.PtnId);
                bool isDoctorOfAppointment = await _context.Doctors.AnyAsync(d => d.AccId == accId && d.DctId == appointment.DctId);

                if (!isPatientOfAppointment && !isDoctorOfAppointment)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem cuộc hẹn này.");
                }

                // 4. Map and return the DTO
                return MapToResponseDTO(appointment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error retrieving personal appointment: {ex.Message}");
                // Re-throw the exception to be handled by the controller or middleware
                throw;
            }
        }

        public async Task<AppointmentResponseDTO> ChangePersonalAppointmentStatusAsync(int accId, int appointmentId, byte status)
        {
            Debug.WriteLine($"Changing personal appointment status for AppointmentId: {appointmentId}, AccountId: {accId} to status: {status}");

            // Validate account existence and authorization
            var account = await _context.Accounts.FindAsync(accId)
                ?? throw new ArgumentException($"Tài khoản với {accId} không tồn tại.");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.ApmId == appointmentId)
                ?? throw new InvalidOperationException($"Cuộc hẹn với ID {appointmentId} không tìm thấy.");

            bool isPatient = await _context.Patients.AnyAsync(p => p.AccId == accId && p.PtnId == appointment.PtnId);
            bool isDoctor = await _context.Doctors.AnyAsync(d => d.AccId == accId && d.DctId == appointment.DctId);

            if (!isPatient && !isDoctor)
                throw new UnauthorizedAccessException("Chỉ có bác sĩ hoặc bệnh nhân của cuộc hẹn này mới có thể thay đổi thông tin.");

            // Delegate to ChangeAppointmentStatusAsync for the rest of the logic
            return await ChangeAppointmentStatusAsync(appointmentId, status, accId);
        }
    }
}