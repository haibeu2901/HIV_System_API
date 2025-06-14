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
                PtnId = requestDTO.PtnId,
                DctId = requestDTO.DctId,
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
                ApmId = appointment.ApmId,
                PtnId = appointment.PtnId,
                PatientName = patient.Acc.Fullname,
                DctId = appointment.DctId,
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
            if (!await _context.PatientMedicalRecords.AnyAsync(r => r.PtnId == request.PtnId))
                throw new ArgumentException("Patient medical record does not exist.");

            // Validate DctId
            if (!await _context.Doctors.AnyAsync(d => d.DctId == request.DctId))
                throw new ArgumentException("Doctor does not exist.");

            // Validate ApmStatus (only for updates)
            if (validateStatus && (request.ApmStatus < 1 || request.ApmStatus > 5))
                throw new ArgumentException("Invalid appointment status. Must be between 1 and 5.");

            // Validate DoctorWorkSchedule
            var dayOfWeek = request.ApmtDate.ToDateTime(TimeOnly.MinValue).DayOfWeek;
            var schedule = await _context.DoctorWorkSchedules
                .FirstOrDefaultAsync(s => s.DoctorId == request.DctId && s.DayOfWeek == (int)dayOfWeek);

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
                .Where(a => a.DctId == request.DctId &&
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

        public async Task<List<AppointmentResponseDTO>> GetAppointmentsByAccountIdAsync(int accId)
        {
            var appointments = await _appointmentRepo.GetAppointmentsByAccountIdAsync(accId);
            return appointments.Select(MapToResponseDTO).ToList();
        }


        public async Task<AppointmentResponseDTO> CreateAppointmentAsync(AppointmentRequestDTO request)
        {
            Debug.WriteLine($"Creating appointment for PmrId: {request.PtnId}, DctId: {request.DctId}");
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request DTO is required.");

            // Validate patient existence
            var patient = await _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefaultAsync(p => p.PtnId == request.PtnId);
            if (patient == null)
                throw new ArgumentException("Patient does not exist.");

            // Validate doctor existence
            var doctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == request.DctId);
            if (doctor == null)
                throw new ArgumentException("Doctor does not exist.");

            // Check for patient medical record, create if missing
            var patientMedicalRecord = await _context.PatientMedicalRecords
                .FirstOrDefaultAsync(r => r.PtnId == request.PtnId);
            if (patientMedicalRecord == null)
            {
                patientMedicalRecord = new PatientMedicalRecord
                {
                    PtnId = request.PtnId
                };
                _context.PatientMedicalRecords.Add(patientMedicalRecord);
                await _context.SaveChangesAsync();
            }

            // Validate appointment (schedule, overlap, etc.)
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
            Debug.WriteLine($"Deleting appointment with ApmId: {id}");
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
