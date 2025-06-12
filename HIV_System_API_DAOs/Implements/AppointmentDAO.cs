using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.Appointment;
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

            // Validate that DctId and PmrId exist in their respective tables
            var doctorExists = await _context.Doctors.AnyAsync(d => d.DctId == appointment.DctId);
            if (!doctorExists)
                throw new ArgumentException("The specified doctor does not exist.", nameof(appointment.DctId));

            var patientRecordExists = await _context.PatientMedicalRecords.AnyAsync(p => p.PmrId == appointment.PmrId);
            if (!patientRecordExists)
                throw new ArgumentException("The specified patient medical record does not exist.", nameof(appointment.PmrId));

            // Set navigation properties to null to avoid validation errors
            appointment.Dct = null;
            appointment.Pmr = null;

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<List<AppointmentDTO>> GetAllAppointmentsAsync()
        {
            var appointments = await _context.Appointments
                .Include(a => a.Dct)
                    .ThenInclude(d => d.Acc)
                .Include(a => a.Pmr)
                    .ThenInclude(p => p.Ptn)
                    .ThenInclude(pt => pt.Acc)
                .Select(a => new AppointmentDTO
                {
                    ApmId = a.ApmId,
                    PmrId = a.PmrId,
                    PatientName = a.Pmr.Ptn.Acc.Fullname,
                    DctId = a.DctId,
                    DoctorName = a.Dct.Acc.Fullname,
                    ApmtDate = a.ApmtDate,
                    ApmTime = a.ApmTime,
                    ApmStatus = a.ApmStatus,
                    Notes = a.Notes
                })
                .ToListAsync();

            return appointments;
        }

        public async Task<AppointmentDTO> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Dct)
                    .ThenInclude(d => d.Acc)
                .Include(a => a.Pmr)
                    .ThenInclude(p => p.Ptn)
                    .ThenInclude(pt => pt.Acc)
                .FirstOrDefaultAsync(a => a.ApmId == id);

            if (appointment == null)
                return null;

            return new AppointmentDTO
            {
                ApmId = appointment.ApmId,
                PmrId = appointment.PmrId,
                PatientName = appointment.Pmr?.Ptn?.Acc?.Fullname,
                DctId = appointment.DctId,
                DoctorName = appointment.Dct?.Acc?.Fullname,
                ApmtDate = appointment.ApmtDate,
                ApmTime = appointment.ApmTime,
                ApmStatus = appointment.ApmStatus,
                Notes = appointment.Notes
            };
        }

        public async Task<bool> UpdateAppointmentByIdAsync(Appointment appointment)
        {
            try
            {
                var existingAppointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.ApmId == appointment.ApmId);

                if (existingAppointment == null)
                    return false;

                // Update all properties
                existingAppointment.PmrId = appointment.PmrId;
                existingAppointment.DctId = appointment.DctId;
                existingAppointment.ApmtDate = appointment.ApmtDate;
                existingAppointment.ApmTime = appointment.ApmTime;
                existingAppointment.ApmStatus = appointment.ApmStatus;
                existingAppointment.Notes = appointment.Notes;

                _context.Appointments.Update(existingAppointment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
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
