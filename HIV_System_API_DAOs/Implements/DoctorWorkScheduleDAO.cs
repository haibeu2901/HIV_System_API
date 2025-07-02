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
        private static DoctorWorkScheduleDAO? _instance;

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
            {
                throw new ArgumentNullException(nameof(doctorWorkSchedule));
            }
            try
            {
                var existingSchedule = await _context.DoctorWorkSchedules
                    .FirstOrDefaultAsync(dws => dws.DoctorId == doctorWorkSchedule.DoctorId &&
                                                dws.WorkDate == doctorWorkSchedule.WorkDate &&
                                                dws.StartTime == doctorWorkSchedule.StartTime &&
                                                dws.EndTime == doctorWorkSchedule.EndTime);

                if (existingSchedule != null)
                {
                    throw new InvalidOperationException("A schedule with the same doctor, date, and time already exists.");
                }

                // Add and save the new schedule
                await _context.DoctorWorkSchedules.AddAsync(doctorWorkSchedule);
                await _context.SaveChangesAsync();

                // Return the created entity with generated ID
                return doctorWorkSchedule;
            }
            catch (DbUpdateException ex)
            {
                // Handle database-specific errors (constraint violations, etc.)
                throw new InvalidOperationException("Failed to create doctor work schedule due to database constraints.", ex);
            }
        }

        public async Task<bool> DeleteDoctorWorkScheduleAsync(int id)
        {
            try
            {
                // Use more efficient approach - directly delete without fetching first
                var rowsAffected = await _context.DoctorWorkSchedules
                    .Where(dws => dws.DwsId == id)
                    .ExecuteDeleteAsync();

                return rowsAffected > 0;
            }
            catch (DbUpdateException ex)
            {
                // Handle database-specific errors (foreign key constraints, etc.)
                throw new InvalidOperationException($"Failed to delete doctor work schedule with ID {id} due to database constraints.", ex);
            }
        }

        public async Task<DoctorWorkSchedule?> GetDoctorWorkScheduleByIdAsync(int id)
        {
            try
            {
                return await _context.DoctorWorkSchedules
                    .Include(dws => dws.Doctor) // Include related Doctor entity if needed
                    .FirstOrDefaultAsync(dws => dws.DwsId == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve doctor work schedule with ID {id}.", ex);
            }
        }

        public async Task<List<DoctorWorkSchedule>> GetDoctorWorkSchedulesAsync()
        {
            try
            {
                return await _context.DoctorWorkSchedules
                    .Include(dws => dws.Doctor) // Include related Doctor entity if needed
                    .OrderBy(dws => dws.WorkDate)
                    .ThenBy(dws => dws.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve doctor work schedules.", ex);
            }
        }

        public async Task<DoctorWorkSchedule> UpdateDoctorWorkScheduleAsync(int id, DoctorWorkSchedule doctorWorkSchedule)
        {
            if (doctorWorkSchedule == null)
                throw new ArgumentNullException(nameof(doctorWorkSchedule));

            try
            {
                var existingSchedule = await _context.DoctorWorkSchedules
                    .Include(dws => dws.Doctor) // Include related Doctor entity if needed
                    .FirstOrDefaultAsync(dws => dws.DwsId == id);

                if (existingSchedule == null)
                    throw new KeyNotFoundException($"Doctor work schedule with ID {id} not found.");

                // Check for conflicts with other schedules for the same doctor
                var conflictingSchedule = await _context.DoctorWorkSchedules
                    .Where(dws => dws.DwsId != id && // Exclude the schedule being updated
                                  dws.DoctorId == doctorWorkSchedule.DoctorId &&
                                  dws.WorkDate == doctorWorkSchedule.WorkDate &&
                                  dws.StartTime == doctorWorkSchedule.StartTime &&
                                  dws.EndTime == doctorWorkSchedule.EndTime)
                    .FirstOrDefaultAsync();

                if (conflictingSchedule != null)
                    throw new InvalidOperationException("A schedule with the same doctor, date, and time already exists.");

                // Update fields
                existingSchedule.DayOfWeek = doctorWorkSchedule.DayOfWeek;
                existingSchedule.WorkDate = doctorWorkSchedule.WorkDate;
                existingSchedule.IsAvailable = doctorWorkSchedule.IsAvailable;
                existingSchedule.StartTime = doctorWorkSchedule.StartTime;
                existingSchedule.EndTime = doctorWorkSchedule.EndTime;
                // Do not update navigation property Doctor directly

                _context.DoctorWorkSchedules.Update(existingSchedule);
                await _context.SaveChangesAsync();
                return existingSchedule;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Failed to update doctor work schedule with ID {id} due to database constraints.", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update doctor work schedule with ID {id}.", ex.InnerException);
            }
        }

        public async Task<List<DoctorWorkSchedule>> GetPersonalWorkSchedulesAsync(int doctorId)
        {
            try
            {
                return await _context.DoctorWorkSchedules
                    .Where(dws => dws.DoctorId == doctorId)
                    .OrderBy(dws => dws.WorkDate)
                    .ThenBy(dws => dws.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve work schedules for doctor with ID {doctorId}.", ex);
            }
        }

        public async Task<List<DoctorWorkSchedule>> GetDoctorWorkSchedulesByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorWorkSchedules
                .Where(dws => dws.DoctorId == doctorId)
                .Include(dws => dws.Doctor)
                .OrderBy(dws => dws.WorkDate)
                .ThenBy(dws => dws.StartTime)
                .ToListAsync();
        }
    }
}
