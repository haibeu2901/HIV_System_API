using Azure.Core;
using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientDTO;
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
            return await _context.Patients
                .Include(p => p.Account)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
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
            {
                return false;
            }

            //// Remove related Account if needed
            //if (patient.Account != null)
            //{
            //    _context.Accounts.Remove(patient.Account);
            //}

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            // Attach Account if not tracked
            if (patient.Account != null)
            {
                _context.Accounts.Attach(patient.Account);
            }

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task<Patient> UpdatePatientAsync(int patientId, Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            var existingPatient = await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (existingPatient == null)
                throw new KeyNotFoundException($"Patient with ID {patientId} not found.");

            // Update Patient properties
            existingPatient.AccId = patient.AccId;

            // Update Account if provided
            if (patient.Account != null && existingPatient.Account != null)
            {
                existingPatient.Account.AccUsername = patient.Account.AccUsername;
                existingPatient.Account.AccPassword = patient.Account.AccPassword;
                existingPatient.Account.Email = patient.Account.Email;
                existingPatient.Account.Fullname = patient.Account.Fullname;
                existingPatient.Account.Dob = patient.Account.Dob;
                existingPatient.Account.Gender = patient.Account.Gender;
                existingPatient.Account.Roles = patient.Account.Roles;
                existingPatient.Account.IsActive = patient.Account.IsActive;
            }

            // Update PatientMedicalRecord if needed (not shown here)

            await _context.SaveChangesAsync();
            return existingPatient;
        }
    }
}
