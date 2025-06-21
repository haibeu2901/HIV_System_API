using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.DoctorDTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class DoctorDAO : IDoctorDAO
    {
        private readonly HivSystemApiContext _context;
        private static DoctorDAO? _instance;


        public DoctorDAO()
        {
            _context = new HivSystemApiContext();
        }
        public static DoctorDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DoctorDAO();
                }
                return _instance;
            }

        }

        public async Task<List<Doctor>> GetAllDoctorsAsync()
        {
            return await _context.Doctors
                .Include(d => d.Acc)
                .Include(d => d.DoctorWorkSchedules)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors
                .Include(d => d.Acc)
                .Include(d => d.DoctorWorkSchedules)
                .FirstOrDefaultAsync(d => d.DctId == id);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            // Attach Acc if not tracked
            if (doctor.Acc != null)
            {
                var existingAcc = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccId == doctor.Acc.AccId);
                if (existingAcc != null)
                {
                    doctor.Acc = existingAcc;
                }
                else
                {
                    _context.Accounts.Attach(doctor.Acc);
                }
            }

            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
            // Load related Acc after save
            await _context.Entry(doctor).Reference(d => d.Acc).LoadAsync();
            return doctor;
        }

        public async Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            var existingDoctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == id);

            if (existingDoctor == null)
                return null;

            // Update Doctor fields
            existingDoctor.Degree = doctor.Degree;
            existingDoctor.Bio = doctor.Bio;

            // Update Acc if provided
            if (doctor.Acc != null)
            {
                if (existingDoctor.Acc == null || existingDoctor.Acc.AccId != doctor.Acc.AccId)
                {
                    var Acc = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == doctor.Acc.AccId);
                    if (Acc != null)
                    {
                        existingDoctor.Acc = Acc;
                        existingDoctor.AccId = Acc.AccId;
                    }
                }
                else
                {
                    existingDoctor.Acc.Email = doctor.Acc.Email;
                    existingDoctor.Acc.Fullname = doctor.Acc.Fullname;
                    existingDoctor.Acc.Dob = doctor.Acc.Dob;
                    existingDoctor.Acc.Gender = doctor.Acc.Gender;
                    existingDoctor.Acc.Roles = doctor.Acc.Roles;
                    existingDoctor.Acc.IsActive = doctor.Acc.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            await _context.Entry(existingDoctor).Reference(d => d.Acc).LoadAsync();
            return existingDoctor;
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Acc)
                .FirstOrDefaultAsync(d => d.DctId == id);

            if (doctor == null)
                return false;

            // Optionally, handle related entities (e.g., DoctorWorkSchedules) if cascade delete is not configured
            var workSchedules = await _context.DoctorWorkSchedules
                .Where(ws => ws.DoctorId == id)
                .ToListAsync();
            if (workSchedules.Any())
            {
                _context.DoctorWorkSchedules.RemoveRange(workSchedules);
            }

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Doctor>> GetDoctorsByDateAndTimeAsync(DateOnly apmtDate, TimeOnly apmTime)
        {
            // Step 1: Determine the day of week (1=Monday, ..., 7=Sunday)
            var dayOfWeek = (int)apmtDate.ToDateTime(TimeOnly.MinValue).DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;

            // Step 2: Define appointment duration and calculate end time
            var appointmentDuration = TimeSpan.FromMinutes(30);
            var apmEndTime = apmTime.Add(appointmentDuration);

            // Step 3: Query for available doctors
            var availableDoctors = await _context.Doctors
                .Include(d => d.Acc)
                .Include(d => d.DoctorWorkSchedules)
                .Where(d => d.DoctorWorkSchedules.Any(ws =>
                    ws.DayOfWeek == dayOfWeek &&
                    ws.StartTime <= apmTime &&
                    ws.EndTime >= apmEndTime &&
                    ws.IsAvailable == true))
                .Where(d => !_context.Appointments.Any(a =>
                    a.DctId == d.DctId &&
                    a.ApmtDate == apmtDate &&
                    a.ApmTime < apmEndTime &&
                    a.ApmTime >= apmTime &&
                    a.ApmStatus != 4))
                .ToListAsync();

            return availableDoctors;
        }
    }
}
