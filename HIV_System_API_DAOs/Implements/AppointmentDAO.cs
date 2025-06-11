using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            if (appointment.Dct == null)
                throw new ArgumentException("The Dct field is required.", nameof(appointment.Dct));
            if (appointment.Dpm == null)
                throw new ArgumentException("The Dpm field is required.", nameof(appointment.Dpm));

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<List<Appointment>> GetAllAppointmentsAsync()
        {
            return await _context.Appointments.ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await _context.Appointments.FirstOrDefaultAsync(a => a.ApmId == id);
        }

        public async Task<bool> UpdateAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.ApmId == id);
            if (appointment == null)
                return false;

            // Attach and mark as modified (if using DTO, map properties here)
            appointment.ApmId = id; // Assuming you want to keep the same ID
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.ApmId == id);
            if (appointment == null)
                return false;

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeAppointmentStatusAsync(int id, byte status)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.ApmId == id);
            if (appointment == null)
                return false;

            appointment.ApmStatus = status;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
