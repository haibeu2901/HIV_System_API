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
                .Include(a => a.Ptn)
                .ToListAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment), "Appointment cannot be null.");
            try
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Successfully created appointment with ApmId: {appointment.ApmId}");
                return appointment;
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
            {
                Debug.WriteLine($"Appointment with ApmId: {id} not found.");
                throw new KeyNotFoundException($"Appointment with ApmId: {id} not found.");
            }
            // Update fields
            existingAppointment.ApmtDate = appointment.ApmtDate;
            existingAppointment.ApmTime = appointment.ApmTime;
            existingAppointment.ApmStatus = appointment.ApmStatus;
            existingAppointment.DctId = appointment.DctId;
            existingAppointment.PtnId = appointment.PtnId;
            try
            {
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Successfully updated appointment with ApmId: {id}");
                return existingAppointment;
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

        public async Task<List<Appointment>> GetAppointmentsByAccountIdAsync(int accId)
        {
            Debug.WriteLine($"Attempting to retrieve appointments for account with AccId: {accId}");
            return await _context.Appointments
                .Include(a => a.Dct)
                .Include(a => a.Ptn)
                .Where(a => a.Ptn.AccId == accId || a.Dct.AccId == accId)
                .ToListAsync();
        }
    }
}
