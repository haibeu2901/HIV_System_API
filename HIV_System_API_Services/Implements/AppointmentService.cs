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
                ?? throw new InvalidOperationException("Associated patient not found.");

            // Fetch doctor name
            var doctor = _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefault(d => d.DctId == appointment.DctId)
                ?? throw new InvalidOperationException("Associated doctor not found.");

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
                ApmStatus = appointment.ApmStatus
            };
        }

        private async Task ValidateAppointmentAsync(AppointmentRequestDTO request, bool validateStatus, int? apmId = null)
        {
            // Validate PatientId
            if (!await _context.Patients.AnyAsync(p => p.PtnId == request.PatientId))
                throw new ArgumentException("Patient does not exist.");

            // Validate DoctorId
            if (!await _context.Doctors.AnyAsync(d => d.DctId == request.DoctorId))
                throw new ArgumentException("Doctor does not exist.");

            // Validate ApmStatus (only for updates)
            if (validateStatus && (request.ApmStatus < 1 || request.ApmStatus > 5))
                throw new ArgumentException("Invalid appointment status. Must be between 1 and 5.");

            // Validate ApmtDate and ApmTime
            var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);
            var nowTime = TimeOnly.FromDateTime(DateTime.UtcNow);

            if (request.ApmtDate < todayDate)
                throw new ArgumentException("Appointment date cannot be in the past.");
            if (request.ApmtDate == todayDate && request.ApmTime < nowTime)
                throw new ArgumentException("Appointment time cannot be in the past for today's date.");

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
                var message = "Doctor does not have an available work schedule on this day.";
                if (nearestDate.HasValue)
                    message += $" Nearest available date: {nearestDate.Value:yyyy-MM-dd}.";
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
                    : "No available slots.";

                throw new InvalidOperationException(
                    $"Requested appointment time is outside of doctor's available schedule. " +
                    $"Doctor's working time slots on {request.ApmtDate:yyyy-MM-dd}: {slotsMessage}."
                );
            }

            // Assume each appointment is 30 minutes
            var appointmentDuration = TimeSpan.FromMinutes(30);
            var apmTime = request.ApmTime;
            var apmEnd = apmTime.Add(appointmentDuration);

            // Fetch all appointments for the doctor on the date (not cancelled)
            var appointmentsOnDate = await _context.Appointments
                .Where(a => a.DctId == request.DoctorId
                    && a.ApmtDate == request.ApmtDate
                    && a.ApmStatus != 4) // 4 = Cancelled
                .ToListAsync();

            // Check for overlapping appointments (not cancelled)
            var hasOverlap = appointmentsOnDate.Any(a =>
            {
                var existingStart = a.ApmTime;
                var existingEnd = a.ApmTime.Add(appointmentDuration);
                return existingStart < apmEnd && existingEnd > apmTime
                    && (!apmId.HasValue || a.ApmId != apmId.Value);
            });

            if (hasOverlap)
            {
                // Find all overlapping appointments for more detailed feedback
                var overlappingAppointments = appointmentsOnDate
                    .Where(a =>
                    {
                        var existingStart = a.ApmTime;
                        var existingEnd = a.ApmTime.Add(appointmentDuration);
                        return existingStart < apmEnd && existingEnd > apmTime
                            && (!apmId.HasValue || a.ApmId != apmId.Value);
                    })
                    .Select(a => new { a.ApmTime })
                    .ToList();

                throw new InvalidOperationException(
                    $"Doctor has an overlapping appointment at this time."
                );
            }
        }

        public async Task<AppointmentResponseDTO> ChangeAppointmentStatusAsync(int id, byte status)
        {
            Debug.WriteLine($"Changing status of appointment with ApmId: {id} to {status}");
            if (status < 1 || status > 5)
                throw new ArgumentException("Invalid appointment status. Must be between 1 and 5.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.ApmId == id);

                if (appointment == null)
                    throw new InvalidOperationException($"Appointment with ID {id} not found.");

                var updatedAppointment = await _appointmentRepo.ChangeAppointmentStatusAsync(id, status);

                if (updatedAppointment == null)
                    throw new InvalidOperationException($"Failed to update appointment status for ID {id}.");

                if (updatedAppointment.ApmStatus == 4) // If status is cancelled
                {
                    // Create notification for cancellation
                    var notification = new Notification
                    {
                        NotiType = "Appt Cancel", // <= 12 chars
                        NotiMessage = "Cuộc hẹn của bạn đã bị huỷ.",
                        SendAt = DateTime.UtcNow
                    };
                    var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);
                    await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, updatedAppointment.PtnId);
                    await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, updatedAppointment.DctId);
                }
                else if (updatedAppointment.ApmStatus == 2 || updatedAppointment.ApmStatus == 3) // If status is confirmed
                {
                    // Create notification for confirmation
                    var notification = new Notification
                    {
                        NotiType = "Appt Confirm", // <= 12 chars
                        NotiMessage = "Cuộc hẹn của bạn đã được xác nhận.",
                        SendAt = DateTime.UtcNow
                    };
                    var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);
                    await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, updatedAppointment.PtnId);
                    await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, updatedAppointment.DctId);
                }

                await transaction.CommitAsync();
                return MapToResponseDTO(updatedAppointment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to change appointment status: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
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
                throw new ArgumentNullException(nameof(request), "Request DTO is required.");

            // Validate patient existence
            var patient = await _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefaultAsync(p => p.PtnId == accId);
            if (patient == null)
                throw new ArgumentException("Patient does not exist.");

            // Validate doctor existence
            var doctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == request.DoctorId);
            if (doctor == null)
                throw new ArgumentException("Doctor does not exist.");

            // Validate appointment (schedule, overlap, etc.)
            var appointmentRequestDto = new AppointmentRequestDTO
            {
                PatientId = accId,
                DoctorId = request.DoctorId,
                ApmtDate = request.AppointmentDate,
                ApmTime = request.AppointmentTime,
                Notes = request.Notes,
                ApmStatus = 1 // Default status for creation
            };
            await ValidateAppointmentAsync(appointmentRequestDto, validateStatus: false);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var createdAppointment = await _appointmentRepo.CreateAppointmentAsync(request, accId);
                await transaction.CommitAsync();

                // Create notification and send to doctor
                var notification = new Notification
                {
                    NotiType = "Appointment Request",
                    NotiMessage = "Có 1 yêu cầu đặt lịch hẹn mới, vui lòng phản hồi.",
                    SendAt = DateTime.UtcNow
                };
                var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);
                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, doctor.AccId);

                return MapToResponseDTO(createdAppointment);
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
                throw new ArgumentNullException(nameof(request), "Appointment update request cannot be null.");

            // Check if the appointment exists
            if (!await _context.Appointments.AnyAsync(a => a.ApmId == id))
                throw new KeyNotFoundException($"Appointment with ID {id} was not found.");

            // Validate the appointment details (throws ArgumentException/InvalidOperationException as needed)
            await ValidateAppointmentAsync(request, validateStatus: true, apmId: id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = MapToEntity(request);
                appointment.ApmId = id;

                var updatedAppointment = await _appointmentRepo.UpdateAppointmentByIdAsync(id, appointment);

                if (updatedAppointment == null)
                    throw new InvalidOperationException($"Failed to update appointment with ID {id} due to a repository error.");

                await transaction.CommitAsync();
                return MapToResponseDTO(updatedAppointment);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("The appointment was modified by another user. Please reload and try again.");
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"A database error occurred while updating the appointment: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"An unexpected error occurred while updating the appointment: {ex.Message}");
            }
        }

        public async Task<AppointmentResponseDTO> UpdateAppointmentRequestAsync(int appointmentId, UpdateAppointmentRequestDTO appointment, int accId)
        {
            Debug.WriteLine($"Updating appointment for AccountId: {accId}");

            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Request DTO is required.");

            // Find the appointment by ID
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.ApmId == appointmentId);

            if (existingAppointment == null)
                throw new InvalidOperationException("Appointment not found for update.");

            // Prepare DTO for validation
            var requestDto = new AppointmentRequestDTO
            {
                PatientId = existingAppointment.PtnId,
                DoctorId = existingAppointment.DctId,
                ApmtDate = appointment.AppointmentDate,
                ApmTime = appointment.AppointmentTime,
                Notes = appointment.Notes,
                ApmStatus = existingAppointment.ApmStatus
            };

            await ValidateAppointmentAsync(requestDto, validateStatus: true, apmId: existingAppointment.ApmId);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Update fields
                existingAppointment.ApmtDate = appointment.AppointmentDate;
                existingAppointment.ApmTime = appointment.AppointmentTime;
                existingAppointment.Notes = appointment.Notes;

                var updatedAppointment = await _appointmentRepo.UpdateAppointmentAsync(appointmentId, appointment, accId);

                var notification = new Notification
                {
                    NotiType = "Appointment Update",
                    NotiMessage = "Lịch hẹn của bạn đã được cập nhật.",
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
                throw new ArgumentException($"Account with ID {accId} does not exist.");

            // Optionally, check if account is a patient or doctor
            var isPatient = await _context.Patients.AnyAsync(p => p.AccId == accId);
            var isDoctor = await _context.Doctors.AnyAsync(d => d.AccId == accId);
            if (!isPatient && !isDoctor)
                throw new InvalidOperationException("Account is neither a patient nor a doctor.");

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
                    throw new InvalidOperationException($"Appointment with ID {appointmentId} not found.");

                // Only allow doctor to complete
                var account = await _context.Accounts.FindAsync(accId);
                if (account == null)
                    throw new ArgumentException($"Account with ID {accId} does not exist.");
                var isDoctor = await _context.Doctors.AnyAsync(d => d.AccId == accId && d.DctId == appointment.DctId);
                if (!isDoctor)
                    throw new UnauthorizedAccessException("Only the doctor of this appointment can complete it.");

                // Only allow if not already completed/cancelled
                if (appointment.ApmStatus == 4)
                    throw new InvalidOperationException("Cannot complete a cancelled appointment.");
                if (appointment.ApmStatus == 5)
                    throw new InvalidOperationException("Appointment is already completed.");

                // Update notes if provided
                if (!string.IsNullOrWhiteSpace(dto.Notes))
                {
                    appointment.Notes = dto.Notes;
                }

                // Change status to completed (5)
                appointment.ApmStatus = 5;
                _context.Appointments.Update(appointment);
                await _context.SaveChangesAsync();

                // Create notification for both patient and doctor
                var notification = new Notification
                {
                    NotiType = "Appt Complete",
                    NotiMessage = "Cuộc hẹn của bạn đã thành công.",
                    SendAt = DateTime.UtcNow
                };
                var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);
                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, appointment.PtnId);
                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, appointment.DctId);

                // Ensure patient medical record exists
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

                await transaction.CommitAsync();
                return MapToResponseDTO(appointment);
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
                    throw new ArgumentException($"Account with ID {accId} does not exist.");
                }

                // 2. Retrieve the appointment
                var appointment = await _appointmentRepo.GetAppointmentByIdAsync(appointmentId);
                if (appointment == null)
                {
                    throw new KeyNotFoundException($"Appointment with ID {appointmentId} not found.");
                }

                // 3. Authorize the user
                // Check if the account belongs to the patient or the doctor of the appointment
                bool isPatientOfAppointment = await _context.Patients.AnyAsync(p => p.AccId == accId && p.PtnId == appointment.PtnId);
                bool isDoctorOfAppointment = await _context.Doctors.AnyAsync(d => d.AccId == accId && d.DctId == appointment.DctId);

                if (!isPatientOfAppointment && !isDoctorOfAppointment)
                {
                    throw new UnauthorizedAccessException("You do not have permission to view this appointment.");
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
                ?? throw new ArgumentException($"Account with ID {accId} does not exist.");

            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.ApmId == appointmentId)
                ?? throw new InvalidOperationException($"Appointment with ID {appointmentId} not found.");

            bool isPatient = await _context.Patients.AnyAsync(p => p.AccId == accId && p.PtnId == appointment.PtnId);
            bool isDoctor = await _context.Doctors.AnyAsync(d => d.AccId == accId && d.DctId == appointment.DctId);

            if (!isPatient && !isDoctor)
                throw new UnauthorizedAccessException("Only the patient or doctor of this appointment can change its status.");

            // Delegate to ChangeAppointmentStatusAsync for the rest of the logic
            return await ChangeAppointmentStatusAsync(appointmentId, status);
        }
    }
}
