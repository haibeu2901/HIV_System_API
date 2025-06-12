using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class PatientDAO : IPatientDAO
    {
        private readonly HivSystemContext _context;
        private static PatientDAO _instance;

        public PatientDAO()
        {
            _context = new HivSystemContext();
        }

        public static PatientDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PatientDAO();
                }
                return _instance;
            }
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients.Include(p => p.Account).ToListAsync();
        }

        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (patient == null)
                return false;

            // Remove related Account if needed
            if (patient.Account != null)
            {
                _context.Accounts.Remove(patient.Account);
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Patient> CreatePatientAsync(int accId)
        {
            // Check if Account exists
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccId == accId);
            if (account == null)
                throw new ArgumentException($"Account with AccId {accId} does not exist.");

            // Create new Patient
            var patient = new Patient
            {
                AccId = accId,
                Account = account
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Optionally reload with Account navigation property
            return await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patient.PtnId);
        }

        public async Task<bool> UpdatePatientAsync(int patientId, Patient updatedPatient)
        {
            if (updatedPatient == null)
                throw new ArgumentNullException(nameof(updatedPatient));

            var existingPatient = await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (existingPatient == null)
                return false;

            if (existingPatient.Account == null)
                throw new InvalidOperationException("The existing patient's account is null.");

            // Update Patient properties
            existingPatient.AccId = updatedPatient.AccId;
            existingPatient.Account.Roles = updatedPatient.Account.Roles;   
            existingPatient.Account.IsActive = updatedPatient.Account.IsActive;

            // Update Account if provided
            if (updatedPatient.Account != null && existingPatient.Account != null)
            {
                existingPatient.Account.AccPassword = updatedPatient.Account.AccPassword;
                existingPatient.Account.Email = updatedPatient.Account.Email;
                existingPatient.Account.Fullname = updatedPatient.Account.Fullname;
                existingPatient.Account.Dob = updatedPatient.Account.Dob;
                existingPatient.Account.Gender = updatedPatient.Account.Gender;
            }

            _context.Patients.Update(existingPatient);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
