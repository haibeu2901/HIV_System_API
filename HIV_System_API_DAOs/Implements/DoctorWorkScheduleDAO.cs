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
    public class DoctorWorkScheduleDAO : IDoctorWorkScheduleDAO
    {
        private readonly HivSystemApiContext _context;
        private static DoctorWorkScheduleDAO _instance;

        public static DoctorWorkScheduleDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DoctorWorkScheduleDAO();
                }
                return _instance;
            }
        }

        public DoctorWorkScheduleDAO()
        {
            _context = new HivSystemApiContext();
        }

        public async Task<DoctorWorkSchedule> CreateDoctorWorkScheduleAsync(DoctorWorkSchedule doctorWorkSchedule)
        {
            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule));

            await _context.DoctorWorkSchedules.AddAsync(doctorWorkSchedule);
            await _context.SaveChangesAsync();
            return doctorWorkSchedule;
        }

        public async Task<bool> DeleteDoctorWorkScheduleAsync(int id)
        {
            var schedule = await _context.DoctorWorkSchedules.FindAsync(id);
            if (schedule == null)
            {
                return false;
            }

            _context.DoctorWorkSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DoctorWorkSchedule?> GetDoctorWorkScheduleByIdAsync(int id)
        {
            return await _context.DoctorWorkSchedules.FindAsync(id);
        }

        public async Task<List<DoctorWorkSchedule>> GetDoctorWorkSchedulesAsync()
        {
            return await _context.DoctorWorkSchedules.ToListAsync();
        }

        public async Task<DoctorWorkSchedule> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkSchedule doctorWorkSchedule)
        {
            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule));

            var existingSchedule = await _context.DoctorWorkSchedules.FindAsync(id);
            if (existingSchedule == null)
                throw new KeyNotFoundException($"DoctorWorkSchedule with id {id} not found.");

            existingSchedule.DayOfWeek = doctorWorkSchedule.DayOfWeek;
            existingSchedule.StartTime = doctorWorkSchedule.StartTime;
            existingSchedule.EndTime = doctorWorkSchedule.EndTime;
            // Do not update navigation property Doctor directly

            _context.DoctorWorkSchedules.Update(existingSchedule);
            await _context.SaveChangesAsync();
            return existingSchedule;
        }
    }
}
