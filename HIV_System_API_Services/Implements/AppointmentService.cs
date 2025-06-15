using Azure.Core;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
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
        private readonly HivSystemApiContext _context;

        public AppointmentService()
        {
            _appointmentRepo = new AppointmentRepo();
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
            var patientMedicalRecord = _context.PatientMedicalRecords
                .FirstOrDefault(r => r.PmrId == appointment.PtnId)
                ?? throw new InvalidOperationException("Associated patient medical record not found.");

            var patient = _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefault(p => p.PtnId == patientMedicalRecord.PtnId)
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
            // Validate PmrId
            if (!await _context.PatientMedicalRecords.AnyAsync(r => r.PtnId == request.PatientId))
                throw new ArgumentException("Patient medical record does not exist.");

            // Validate DctId
            if (!await _context.Doctors.AnyAsync(d => d.DctId == request.DoctorId))
                throw new ArgumentException("Doctor does not exist.");

            // Validate ApmStatus (only for updates)
            if (validateStatus && (request.ApmStatus < 1 || request.ApmStatus > 5))
                throw new ArgumentException("Invalid appointment status. Must be between 1 and 5.");

            // Validate DoctorWorkSchedule
            var dayOfWeek = request.ApmtDate.ToDateTime(TimeOnly.MinValue).DayOfWeek;
            var schedule = await _context.DoctorWorkSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == request.DoctorId && s.DayOfWeek == (int)dayOfWeek);

            if (schedule == null)
                throw new InvalidOperationException($"Doctor is not available on {dayOfWeek}.");

            var startTime = schedule.StartTime;
            var endTime = schedule.EndTime;
            var apmTime = request.ApmTime;

            if (apmTime < startTime || apmTime >= endTime)
                throw new InvalidOperationException($"Appointment time is outside doctor's schedule ({startTime}-{endTime}).");

            // Check for overlapping appointments
            var appointmentDuration = TimeSpan.FromMinutes(30); // Assume 30-minute duration
            var endTimeSpan = apmTime.ToTimeSpan() + appointmentDuration;

            var overlapQuery = _context.Appointments
                .Where(a => a.DctId == request.DoctorId &&
                            a.ApmtDate == request.ApmtDate &&
                            a.ApmTime < TimeOnly.FromTimeSpan(endTimeSpan) &&
                            a.ApmTime >= apmTime &&
                            a.ApmStatus != 4); // Exclude cancelled appointments

            if (apmId.HasValue)
                overlapQuery = overlapQuery.Where(a => a.ApmId != apmId.Value); // Exclude current appointment for updates

            var hasOverlap = await overlapQuery.AnyAsync();

            if (hasOverlap)
                throw new InvalidOperationException("Doctor has an overlapping appointment at this time.");
        }

        public async Task<AppointmentResponseDTO> ChangeAppointmentStatusAsync(int id, byte status)
        {
            Debug.WriteLine($"Changing status of appointment with ApmId: {id} to {status}");
            if (status < 1 || status > 5)
                throw new ArgumentException("Invalid appointment status. Must be between 1 and 5.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var updatedAppointment = await _appointmentRepo.ChangeAppointmentStatusAsync(id, status);
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

        public async Task<AppointmentResponseDTO> CreateAppointmentAsync(AppointmentRequestDTO request)
        {
            Debug.WriteLine($"Creating appointment for PmrId: {request.PatientId}, DctId: {request.DoctorId}");
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request DTO is required.");

            await ValidateAppointmentAsync(request, validateStatus: false);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = MapToEntity(request);
                appointment.ApmStatus = 1;
                var createdAppointment = await _appointmentRepo.CreateAppointmentAsync(appointment);
                await transaction.CommitAsync();
                return MapToResponseDTO(createdAppointment);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create appointment: {ex.Message}");
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
                throw new ArgumentNullException(nameof(request), "Request DTO is required.");

            if (!await _context.Appointments.AnyAsync(a => a.ApmId == id))
                throw new InvalidOperationException($"Appointment with ID {id} not found.");

            await ValidateAppointmentAsync(request, validateStatus: true, apmId: id);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var appointment = MapToEntity(request);
                appointment.ApmId = id;
                var updatedAppointment = await _appointmentRepo.UpdateAppointmentByIdAsync(id, appointment);
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

    }
}
