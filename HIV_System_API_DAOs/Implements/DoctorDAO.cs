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
        private readonly HivSystemContext _context;
        private static DoctorDAO _instance;


        public DoctorDAO()
        {
            _context = new HivSystemContext();
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
                .Include(d => d.Account)
                .Include(d => d.Doct  orWorkSchedules)
                .ToListAsync();
        }

        public async Task<Doctor?> GetDoctorByIdAsync(int id)
        {
            return await _context.Doctors
                .Include(d => d.Account)
                .Include(d => d.DoctorWorkSchedules)
                .FirstOrDefaultAsync(d => d.DctId == id);
        }

        public async Task<Doctor> CreateDoctorAsync(Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            // Attach Account if not tracked
            if (doctor.Account != null)
            {
                var existingAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccId == doctor.Account.AccId);
                if (existingAccount != null)
                {
                    doctor.Account = existingAccount;
                }
                else
                {
                    _context.Accounts.Attach(doctor.Account);
                }
            }

            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();
            // Load related Account after save
            await _context.Entry(doctor).Reference(d => d.Account).LoadAsync();
            return doctor;
        }

        public async Task<Doctor?> UpdateDoctorAsync(int id, Doctor doctor)
        {
            if (doctor == null)
                throw new ArgumentNullException(nameof(doctor));

            var existingDoctor = await _context.Doctors
                .Include(d => d.Account)
                .FirstOrDefaultAsync(d => d.DctId == id);

            if (existingDoctor == null)
                return null;

            // Update Doctor fields
            existingDoctor.Degree = doctor.Degree;
            existingDoctor.Bio = doctor.Bio;

            // Update Account if provided
            if (doctor.Account != null)
            {
                if (existingDoctor.Account == null || existingDoctor.Account.AccId != doctor.Account.AccId)
                {
                    var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == doctor.Account.AccId);
                    if (account != null)
                    {
                        existingDoctor.Account = account;
                        existingDoctor.AccId = account.AccId;
                    }
                }
                else
                {
                    existingDoctor.Account.Email = doctor.Account.Email;
                    existingDoctor.Account.Fullname = doctor.Account.Fullname;
                    existingDoctor.Account.Dob = doctor.Account.Dob;
                    existingDoctor.Account.Gender = doctor.Account.Gender;
                    existingDoctor.Account.Roles = doctor.Account.Roles;
                    existingDoctor.Account.IsActive = doctor.Account.IsActive;
                }
            }

            await _context.SaveChangesAsync();
            await _context.Entry(existingDoctor).Reference(d => d.Account).LoadAsync();
            return existingDoctor;
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Account)
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
    }
}
