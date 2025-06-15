using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.AppointmentDTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class AppointmentDAO : IAppointmentDAO
    {
        private readonly HivSystemApiContext _context;
        private static AppointmentDAO? _instance;

        public AppointmentDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static AppointmentDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppointmentDAO();
                }
                return _instance;
            }
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Dct)
                .Include(a => a.Ptn)
                    .ThenInclude(d => d.Acc)
                .Include(a => a.Ptn)
                    .ThenInclude(p => p.Acc)
                .ToListAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(CreateAppointmentRequestDTO appointmentDto, int accId)
        {
            if (appointmentDto == null)
                throw new ArgumentNullException(nameof(appointmentDto), "Appointment cannot be null.");

            // Ensure doctor exists
            var doctor = await _context.Doctors.FindAsync(appointmentDto.DoctorId);
            if (doctor == null)
                throw new InvalidOperationException($"Doctor with ID {appointmentDto.DoctorId} not found.");

            // Ensure patient exists (by account)
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.AccId == accId);
            if (patient == null)
                throw new InvalidOperationException($"Patient not found for account ID {accId}.");

            // Create Appointment entity
            var appointment = new Appointment
            {
                DctId = doctor.DctId,
                PtnId = patient.PtnId,
                ApmtDate = appointmentDto.AppointmentDate,
                ApmTime = appointmentDto.AppointmentTime,
                Notes = appointmentDto.Notes,
                ApmStatus = 1 // Default status, adjust as needed
            };

            try
            {
                await _context.Appointments.AddAsync(appointment);
                await _context.SaveChangesAsync();

                // Return appointment with related entities loaded
                var created = await _context.Appointments
                    .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                    .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                    .FirstOrDefaultAsync(a => a.ApmId == appointment.ApmId);

                return created ?? throw new InvalidOperationException("Failed to retrieve created appointment.");
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to create appointment: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to create appointment due to database error.", ex);
            }
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            Debug.WriteLine($"Attempting to retrieve appointment with ApmId: {id}");
            var appointment = await _context.Appointments
                .Include(a => a.Dct)
                .Include(a => a.Ptn)
                .FirstOrDefaultAsync(a => a.ApmId == id);
            if (appointment == null)
            {
                Debug.WriteLine($"Appointment with ApmId: {id} not found.");
            }
            else
            {
                Debug.WriteLine($"Successfully retrieved appointment with ApmId: {id}");
            }
            return appointment;
        }

        public async Task<Appointment> UpdateAppointmentByIdAsync(int id, Appointment appointment)
        {
            Debug.WriteLine($"Attempting to update appointment with ApmId: {id}");
            var existingAppointment = await _context.Appointments
                .Include(a => a.Dct)
                .Include(a => a.Ptn)
                .FirstOrDefaultAsync(a => a.ApmId == id);
            if (existingAppointment == null)
                throw new KeyNotFoundException($"Appointment with id {id} not found.");

            // Update fields
            existingAppointment.ApmtDate = appointment.ApmtDate;
            existingAppointment.ApmTime = appointment.ApmTime;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.ApmStatus = appointment.ApmStatus;

            // Only update DctId if it's different and the new doctor exists
            if (appointment.DctId != existingAppointment.DctId)
            {
                var newDoctor = await _context.Doctors.FindAsync(appointment.DctId);
                if (newDoctor == null)
                    throw new InvalidOperationException($"Doctor with ID {appointment.DctId} not found.");
                existingAppointment.DctId = appointment.DctId;
            }

            // Only update PtnId if it's different and the new patient exists
            if (appointment.PtnId != existingAppointment.PtnId)
            {
                var newPatient = await _context.Patients.FindAsync(appointment.PtnId);
                if (newPatient == null)
                    throw new InvalidOperationException($"Patient with ID {appointment.PtnId} not found.");
                existingAppointment.PtnId = appointment.PtnId;
            }

            try
            {
                await _context.SaveChangesAsync();

                // Reload the appointment with related entities
                return await GetAppointmentByIdAsync(id)
                    ?? throw new InvalidOperationException("Failed to retrieve updated appointment.");
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to update appointment: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to update appointment due to database error.", ex);
            }
        }

        public async Task<bool> DeleteAppointmentByIdAsync(int id)
        {
            Debug.WriteLine($"Attempting to delete appointment with ApmId: {id}");
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
            {
                Debug.WriteLine($"Appointment with ApmId: {id} not found.");
                return false;
            }

            _context.Appointments.Remove(appointment);
            try
            {
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Successfully deleted appointment with ApmId: {id}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to delete appointment: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to delete appointment due to database error.", ex);
            }
        }

        public async Task<Appointment> ChangeAppointmentStatusAsync(int id, byte status)
        {
            var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.ApmId == id)
            ?? throw new InvalidOperationException($"Appointment with ApmId: {id} not found.");

            appointment.ApmStatus = status;
            try
            {
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Changed status of appointment with ApmId: {id} to {status}");
                return appointment;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to change appointment status: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to change appointment status due to database error.", ex);
            }
        }

        public async Task<List<Appointment>> GetAppointmentsByAccountIdAsync(int accountId, byte role)
        {
            try
            {
                var query = _context.Appointments
                    .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                    .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                    .AsQueryable();

                if (role == 2) // Doctor
                {
                    // Get doctor's ID from account ID
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.AccId == accountId);

                    if (doctor == null)
                        throw new KeyNotFoundException($"Doctor not found for account ID {accountId}");

                    query = query.Where(a => a.DctId == doctor.DctId);
                }
                else if (role == 3) // Patient
                {
                    // Get patient's ID from account ID
                    var patient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.AccId == accountId);

                    if (patient == null)
                        throw new KeyNotFoundException($"Patient not found for account ID {accountId}");

                    query = query.Where(a => a.PtnId == patient.PtnId);
                }
                else
                {
                    throw new UnauthorizedAccessException("Invalid role for appointment access");
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetAppointmentsByAccountIdAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<Appointment> UpdateAppointmentAsync(int appointmentId, UpdateAppointmentRequestDTO appointmentDto, int accId)
        {
            if (appointmentDto == null)
                throw new ArgumentNullException(nameof(appointmentDto), "Appointment update data cannot be null.");

            // Find the appointment by its ID
            var appointment = await _context.Appointments
                .Include(a => a.Dct)
                .Include(a => a.Ptn)
                .FirstOrDefaultAsync(a => a.ApmId == appointmentId);

            if (appointment == null)
                throw new KeyNotFoundException($"Appointment with id {appointmentId} not found.");

            // Update fields
            appointment.ApmtDate = appointmentDto.AppointmentDate;
            appointment.ApmTime = appointmentDto.AppointmentTime;
            appointment.Notes = appointmentDto.Notes;

            try
            {
                await _context.SaveChangesAsync();
                // Reload with related entities
                var updated = await _context.Appointments
                    .Include(a => a.Dct)
                        .ThenInclude(d => d.Acc)
                    .Include(a => a.Ptn)
                        .ThenInclude(p => p.Acc)
                    .FirstOrDefaultAsync(a => a.ApmId == appointment.ApmId);

                return updated ?? throw new InvalidOperationException("Failed to retrieve updated appointment.");
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to update appointment: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to update appointment due to database error.", ex);
            }
        }
    }
}
