using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.Appointment;
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
                    .ThenInclude(d => d.Acc)
                .Include(a => a.Ptn)
                    .ThenInclude(p => p.Acc)
                .ToListAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            // Verify that the Doctor exists
            var doctor = await _context.Doctors.FindAsync(appointment.DctId);
            if (doctor == null)
                throw new InvalidOperationException($"Doctor with ID {appointment.DctId} not found.");

            // Verify that the Patient exists
            var patient = await _context.Patients.FindAsync(appointment.PtnId);
            if (patient == null)
                throw new InvalidOperationException($"Patient with ID {appointment.PtnId} not found.");

            try
            {
                await _context.Appointments.AddAsync(appointment);
                await _context.SaveChangesAsync();

                // Reload the appointment with related entities
                return await GetAppointmentByIdAsync(appointment.ApmId)
                    ?? throw new InvalidOperationException("Failed to retrieve created appointment.");
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to create appointment: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException("Failed to create appointment due to database error.", ex);
            }
        }

        public async Task<Appointment?> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Dct)
                    .ThenInclude(d => d.Acc)
                .Include(a => a.Ptn)
                    .ThenInclude(p => p.Acc)
                .FirstOrDefaultAsync(a => a.ApmId == id);
        }

        public async Task<Appointment> UpdateAppointmentByIdAsync(int id, Appointment appointment)
        {
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
    }
}
