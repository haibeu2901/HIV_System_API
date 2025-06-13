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
        private readonly HivSystemContext _context;
        private static AppointmentDAO _instance;

        public AppointmentDAO()
        {
            _context = new HivSystemContext();
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
                .Include(a => a.Pmr).ThenInclude(pmr => pmr.Ptn).ThenInclude(ptn => ptn.Account)
                .Include(a => a.Dct).ThenInclude(dct => dct.Account)
                .ToListAsync();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            Debug.WriteLine($"Creating appointment for PmrId: {appointment.PmrId}, DctId: {appointment.DctId}");
            _context.Appointments.Add(appointment);
            try
            {
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Created appointment with ApmId: {appointment.ApmId}");
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
            return await _context.Appointments
            .Include(a => a.Pmr).ThenInclude(pmr => pmr.Ptn).ThenInclude(p => p.Account)
            .Include(a => a.Dct).ThenInclude(d => d.Account)
            .FirstOrDefaultAsync(a => a.ApmId == id);
        }

        public async Task<Appointment> UpdateAppointmentByIdAsync(int id, Appointment appointment)
        {
            var existingAppointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.ApmId == id)
            ?? throw new InvalidOperationException($"Appointment with ApmId: {appointment.ApmId} not found.");

            existingAppointment.PmrId = appointment.PmrId;
            existingAppointment.DctId = appointment.DctId;
            existingAppointment.ApmtDate = appointment.ApmtDate;
            existingAppointment.ApmTime = appointment.ApmTime;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.ApmStatus = appointment.ApmStatus;

            try
            {
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Updated appointment with ApmId: {appointment.ApmId}");
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

        public async Task<List<Appointment>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            return await _context.Appointments
            .Include(a => a.Pmr).ThenInclude(pmr => pmr.Ptn).ThenInclude(p => p.Account)
            .Include(a => a.Dct).ThenInclude(d => d.Account)
            .Where(a => a.DctId == doctorId)
            .ToListAsync();
        }
    }
}
