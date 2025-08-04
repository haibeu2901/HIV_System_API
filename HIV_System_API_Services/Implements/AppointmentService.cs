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
        private readonly NotificationService _notificationService;
        private readonly HivSystemApiContext _context;

        public AppointmentService()
        {
            _appointmentRepo = new AppointmentRepo();
            _notificationRepo = new NotificationRepo();
            _notificationService = new NotificationService();
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
                var todayDate = DateOnly.FromDateTime(DateTime.Now);
                var nowTime = TimeOnly.FromDateTime(DateTime.Now);

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

                // Fetch all appointments for the doctor on the date (not cancelled or completed)
                // Only check appointments that have actual scheduled dates/times (not null)
                var appointmentsOnDate = await _context.Appointments
                    .Where(a => a.DctId == request.DoctorId
                        && a.ApmtDate == request.ApmtDate
                        && a.ApmtDate.HasValue
                        && a.ApmTime.HasValue
                        && (a.ApmStatus != 4 && a.ApmStatus != 5)) // 4 = Cancelled, 5 = Completed
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

                // Check if patient already has an appointment on the same date
                var patientAppointmentsOnDate = await _context.Appointments
                    .Where(a => a.PtnId == request.PatientId
                        && a.ApmtDate == request.ApmtDate
                        && a.ApmtDate.HasValue
                        && a.ApmStatus != 4 && a.ApmStatus != 5 // 4 = Cancelled, 5 = Completed
                        && (!apmId.HasValue || a.ApmId != apmId.Value))
                    .CountAsync();

                if (patientAppointmentsOnDate > 0)
                {
                    throw new InvalidOperationException("Bệnh nhân không được đặt quá 1 cuộc hẹn trong cùng một ngày.");
                }

                if (request.ApmtDate != default(DateOnly) && request.ApmTime != default(TimeOnly))
                {
                    var requestedDate = request.ApmtDate;
                    int diff = (dayOfWeek == 0 ? 7 : dayOfWeek) - 1;
                    var weekStart = requestedDate.AddDays(-diff);
                    var weekEnd = weekStart.AddDays(6);

                    // Count patient's appointments in the same week (excluding cancelled/completed and the current appointment if updating)
                    var patientAppointmentsThisWeek = await _context.Appointments
                        .Where(a => a.PtnId == request.PatientId
                            && a.ApmtDate.HasValue
                            && a.ApmtDate.Value >= weekStart
                            && a.ApmtDate.Value <= weekEnd
                            && a.ApmStatus != 4 && a.ApmStatus != 5 // 4 = Cancelled, 5 = Completed
                            && (!apmId.HasValue || a.ApmId != apmId.Value))
                        .CountAsync();

                    if (patientAppointmentsThisWeek >= 3)
                    {
                        throw new InvalidOperationException("Bệnh nhân không được đặt quá 3 cuộc hẹn trong cùng một tuần.");
                    }
                }
            }
        }

        private async Task ValidateRescheduledAppointmentAsync(AppointmentRequestDTO request, bool validateStatus, int? apmId = null)
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
            if (request.ApmtDate != default(DateOnly) && request.ApmTime != default(TimeOnly))
            {
                // Validate ApmtDate and ApmTime
                var todayDate = DateOnly.FromDateTime(DateTime.Now);
                var nowTime = TimeOnly.FromDateTime(DateTime.Now);

                if (request.ApmtDate < todayDate)
                    throw new ArgumentException("Ngày hẹn không được ở trong quá khứ.");
                if (request.ApmtDate == todayDate && request.ApmTime < nowTime)
                    throw new ArgumentException("Thời gian hẹn không được ở trong quá khứ đối với ngày hôm nay.");

                // Check if the requested reschedule date is within doctor's work schedule range
                var doctorWorkScheduleRange = await _context.DoctorWorkSchedules
                    .Where(s => s.DoctorId == request.DoctorId && s.IsAvailable)
                    .Select(s => s.WorkDate)
                    .ToListAsync();

                if (doctorWorkScheduleRange.Any())
                {
                    var minWorkDate = doctorWorkScheduleRange.Min();
                    var maxWorkDate = doctorWorkScheduleRange.Max();

                    if (request.ApmtDate < minWorkDate || request.ApmtDate > maxWorkDate)
                    {
                        throw new InvalidOperationException("Ngày hẹn tái khám nằm ngoài lịch làm việc hiện tại của bác sĩ, vui lòng ghi chú lại và đặt lại lịch hẹn tái khám sau khi cập nhật mới lịch làm việc.");
                    }
                }

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

                // Fetch all appointments for the doctor on the date (not cancelled or completed)
                // Only check appointments that have actual scheduled dates/times (not null)
                var appointmentsOnDate = await _context.Appointments
                    .Where(a => a.DctId == request.DoctorId
                        && a.ApmtDate == request.ApmtDate
                        && a.ApmtDate.HasValue
                        && a.ApmTime.HasValue
                        && (a.ApmStatus != 4 && a.ApmStatus != 5)) // 4 = Cancelled, 5 = Completed
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
                // Load appointment with all necessary related data in a single optimized query
                var appointment = await _context.Appointments
                    .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                    .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                    .AsNoTracking() // Add for read-only initial query
                    .FirstOrDefaultAsync(a => a.ApmId == id);

                if (appointment == null)
                    throw new InvalidOperationException($"Không tìm thấy cuộc hẹn với ID {id}.");

                // Time validation for cancellation
                if (status == 4 && appointment.ApmtDate.HasValue && appointment.ApmTime.HasValue)
                {
                    var appointmentDateTime = appointment.ApmtDate.Value.ToDateTime(appointment.ApmTime.Value);
                    if ((appointmentDateTime - DateTime.Now).TotalHours < 12)
                        throw new InvalidOperationException("Không thể thay đổi trạng thái cuộc hẹn vì thời gian hẹn nhỏ hơn 12 giờ từ bây giờ.");
                }

                // Authorization check for completion
                if (status == 5 && appointment.DctId != accId)
                    throw new UnauthorizedAccessException("Chỉ bác sĩ của cuộc hẹn này mới có thể hoàn thành nó.");

                // Handle appointment confirmation (status 2 or 3)
                var updatedAppointment = await _context.Appointments.FindAsync(id);
                if ((status == 2 || status == 3) && updatedAppointment.RequestDate.HasValue && updatedAppointment.RequestTime.HasValue)
                {
                    if (updatedAppointment.RequestBy == accId)
                        throw new InvalidOperationException("Bạn không thể thay đổi trạng thái cuộc hẹn mà chính bạn đã tạo.");

                    // Check availability only when confirming
                    var requestDto = new AppointmentRequestDTO
                    {
                        PatientId = updatedAppointment.PtnId,
                        DoctorId = updatedAppointment.DctId,
                        ApmtDate = updatedAppointment.RequestDate.Value,
                        ApmTime = updatedAppointment.RequestTime.Value,
                        Notes = updatedAppointment.Notes,
                        ApmStatus = status
                    };

                    await ValidateAppointmentAsync(requestDto, validateStatus: true, apmId: id);

                    updatedAppointment.ApmtDate = updatedAppointment.RequestDate;
                    updatedAppointment.ApmTime = updatedAppointment.RequestTime;
                }

                // Update status
                updatedAppointment.ApmStatus = status;
                await _appointmentRepo.UpdateAppointmentByIdAsync(updatedAppointment.ApmId, updatedAppointment);
                await _context.SaveChangesAsync();

                // Create and send notification in a single operation
                var notification = new Notification
                {
                    NotiType = GetNotificationTypeForStatus(status),
                    NotiMessage = CreateNotificationMessage(appointment, status),
                    SendAt = DateTime.Now,
                    NotificationAccounts = new List<NotificationAccount>
                    {
                        new NotificationAccount { AccId = appointment.PtnId },
                        new NotificationAccount { AccId = appointment.DctId }
                    }
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Load updated appointment data for response
                var result = await _context.Appointments
                    .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                    .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ApmId == id);

                return MapToResponseDTO(result);
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
                ApmStatus = 2 // Default to confirmed status
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
                    SendAt = DateTime.Now
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
                existingAppointment.ApmStatus = 1;

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
                    SendAt = DateTime.Now,
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

                // Can not complete future appointments
                if (appointment.ApmtDate.HasValue && appointment.ApmTime.HasValue)
                {
                    var today = DateOnly.FromDateTime(DateTime.Now);
                    var now = TimeOnly.FromDateTime(DateTime.Now);

                    // Future date OR (same date but future time)
                    if (appointment.ApmtDate.Value > today ||
                        (appointment.ApmtDate.Value == today && appointment.ApmTime.Value > now))
                    {
                        throw new InvalidOperationException("Không thể hoàn thành cuộc hẹn trong tương lai.");
                    }
                }
                // Update notes if provided
                if (!string.IsNullOrWhiteSpace(dto.Notes))
                {
                    appointment.Notes = dto.Notes;
                    await _appointmentRepo.UpdateAppointmentByIdAsync(appointmentId, appointment);
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
                var result = await _appointmentRepo.ChangeAppointmentStatusAsync(appointmentId, 5); // 5 = Completed

                return MapToResponseDTO(result);
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

        public async Task<AppointmentResponseDTO> CancelPastDateAppointmentsAsync()
        {
            Debug.WriteLine("Cancelling past date appointments");
            // Get all appointments that are in the past and not cancelled or completed
            var pastAppointments = await _context.Appointments
                .Where(a => a.ApmtDate < DateOnly.FromDateTime(DateTime.Now) && (a.ApmStatus != 4 && a.ApmStatus != 5))
                .ToListAsync();
            if (pastAppointments.Count == 0)
            {
                Debug.WriteLine("No past appointments to cancel.");
                return null; // No past appointments to cancel
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var appointment in pastAppointments)
                {
                    appointment.ApmStatus = 4; // Set status to Cancelled
                    await _appointmentRepo.UpdateAppointmentByIdAsync(appointment.ApmId, appointment);
                    // Create notification for cancellation
                    if (appointment.ApmStatus == 3)
                    {
                        var notification = new Notification
                        {
                            NotiType = "Hủy tái khám",
                            NotiMessage = $"Lịch tái khám vào {appointment.ApmtDate:yyyy-MM-dd} lúc {appointment.ApmTime:HH:mm} đã bị hủy do quá hạn, vui lòng đặt lại lịch tái khám vào thời gian gần nhất.",
                            SendAt = DateTime.Now,
                            NotificationAccounts = new List<NotificationAccount>
                        {
                            new NotificationAccount { AccId = appointment.PtnId },
                            new NotificationAccount { AccId = appointment.DctId }
                        }
                        };
                        _context.Notifications.Add(notification);
                    }
                    else
                    {
                        var notification = new Notification
                        {
                            NotiType = "Hủy lịch hẹn",
                            NotiMessage = $"Cuộc hẹn vào {appointment.ApmtDate:yyyy-MM-dd} lúc {appointment.ApmTime:HH:mm} đã bị hủy do quá hạn.",
                            SendAt = DateTime.Now,
                            NotificationAccounts = new List<NotificationAccount>
                        {
                            new NotificationAccount { AccId = appointment.PtnId },
                            new NotificationAccount { AccId = appointment.DctId }
                        }
                        };
                        _context.Notifications.Add(notification);
                    }

                }
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return MapToResponseDTO(pastAppointments.First()); // Return the first cancelled appointment as an example
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to cancel past date appointments: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<AppointmentResponseDTO>> SendNearDateAppointmentAsync(int daysBefore)
        {
            if (daysBefore < 0)
                throw new ArgumentException("Số ngày trước cuộc hẹn phải lớn hơn 0.", nameof(daysBefore));

            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                var targetDate = currentDate.AddDays(daysBefore);
                var responseDTOs = new List<AppointmentResponseDTO>();

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var appointments = await _context.Appointments
                        .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                        .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                        .Where(a => a.ApmtDate.HasValue
                            && a.ApmtDate.Value <= targetDate
                            && (a.ApmStatus == 2 || a.ApmStatus == 3))
                        .ToListAsync();

                    if (!appointments.Any())
                    {
                        return responseDTOs;
                    }

                    // Get today's notifications for appointment reminders
                    var todayStart = DateTime.Now.Date;
                    var todayEnd = todayStart.AddDays(1).AddTicks(-1);

                    var existingReminders = await _context.Notifications
                        .Where(n => n.NotiType == "Nhắc nhở lịch hẹn"
                            && n.SendAt >= todayStart
                            && n.SendAt <= todayEnd)
                        .Select(n => n.NotiMessage)
                        .ToListAsync();

                    foreach (var appointment in appointments)
                    {
                        if (appointment.Ptn?.Acc == null || appointment.Dct?.Acc == null)
                            continue;

                        var reminderMessage = $"Bạn có cuộc hẹn {(appointment.ApmStatus == 3 ? "tái khám" : "")} vào ngày {appointment.ApmtDate:dd/MM/yyyy} " +
                        $"lúc {appointment.ApmTime:HH:mm} với bác sĩ {appointment.Dct.Acc.Fullname}. " +
                        "Vui lòng đến đúng giờ.";

                        // Skip if reminder already sent today
                         if (existingReminders.Any(msg => msg == reminderMessage))
                        {
                            continue;
                        }
                        else
                        {
                            var notificationRequest = new CreateNotificationRequestDTO
                            {
                                NotiType = "Nhắc nhở lịch hẹn",
                                NotiMessage = reminderMessage,
                                SendAt = DateTime.Now
                            };

                            var response = await _notificationService.CreateAndSendToAccountIdAsync(notificationRequest, appointment.Ptn.AccId);
                            responseDTOs.Add(MapToResponseDTO(appointment));
                        }

                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return responseDTOs;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Lỗi khi gửi thông báo nhắc nhở lịch hẹn: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi gửi thông báo nhắc nhở lịch hẹn: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi gửi thông báo nhắc nhở lịch hẹn: {ex.Message}");
            }
        }

        public async Task<AppointmentResponseDTO> RescheduleAppointmentAsync(CreateRescheduleAppointmentRequestDTO appointment, int accId)
        {
            Debug.WriteLine($"Creating appointment for PatientId: {appointment.PatientId}, DoctorId: {accId}");
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Yêu cầu DTO là bắt buộc.");

            var patient = await _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefaultAsync(p => p.PtnId == appointment.PatientId);
            if (patient == null)
                throw new ArgumentException("Bệnh nhân không tồn tại.");

            var doctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == accId);
            if (doctor == null)
                throw new ArgumentException("Bác sĩ không tồn tại.");

            var appointmentRequestDto = new AppointmentRequestDTO
            {
                PatientId = appointment.PatientId,
                DoctorId = accId,
                ApmtDate = appointment.AppointmentDate,
                ApmTime = appointment.AppointmentTime,
                Notes = appointment.Notes,
                ApmStatus = 3 // Default to re-scheduled status
            };
            await ValidateRescheduledAppointmentAsync(appointmentRequestDto, validateStatus: false);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var createdAppointment = await _appointmentRepo.CreateRescheduleAppointmentAsync(appointment, accId);
                var appointmentDto = MapToResponseDTO(createdAppointment);
                var doctorName = appointmentDto.DoctorName;
                var patientName = appointmentDto.PatientName;

                var notification = new Notification
                {
                    NotiType = "Lịch hẹn tái khám",
                    NotiMessage = $"Cuộc hẹn tái khám của bạn vào {createdAppointment.RequestDate:yyyy-MM-dd} lúc {createdAppointment.RequestTime:HH:mm} giữa bệnh nhân {patientName} với bác sĩ {doctorName} đã được tạo.",
                    SendAt = DateTime.Now
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
    }
}