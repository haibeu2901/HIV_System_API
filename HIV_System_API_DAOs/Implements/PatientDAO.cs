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

        public async Task<List<PatientResponseDTO>> GetAllPatientsAsync()
        {
            return await _context.Patients
            .Include(p => p.Account)
            .Select(p => new PatientResponseDTO
            {
                PtnId = p.PtnId,
                AccId = p.AccId,
                Account = new AccountResponseDTO
                {
                    AccId = p.Account.AccId,
                    AccUsername = p.Account.AccUsername,
                    AccPassword = p.Account.AccPassword,
                    Email = p.Account.Email,
                    Fullname = p.Account.Fullname,
                    Dob = p.Account.Dob,
                    Gender = p.Account.Gender,
                    Roles = p.Account.Roles,
                    IsActive = p.Account.IsActive
                }
            })
            .ToListAsync();
        }

        public async Task<PatientResponseDTO> GetPatientByIdAsync(int patientId)
        {
            var patient = await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (patient == null)
                return null;

            return new PatientResponseDTO
            {
                PtnId = patient.PtnId,
                AccId = patient.AccId,
                Account = new AccountResponseDTO
                {
                    AccId = patient.Account.AccId,
                    AccUsername = patient.Account.AccUsername,
                    AccPassword = patient.Account.AccPassword,
                    Email = patient.Account.Email,
                    Fullname = patient.Account.Fullname,
                    Dob = patient.Account.Dob,
                    Gender = patient.Account.Gender,
                    Roles = patient.Account.Roles,
                    IsActive = patient.Account.IsActive
                }
            };
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
                return false;

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PatientResponseDTO> CreatePatientAsync(PatientRequestDTO patientRequest)
        {
            // Pseudocode:
            // 1. Find the Account by patientRequest.AccId.
            // 2. If not found, return null.
            // 3. Create a new Patient and assign the Account.
            // 4. Add Patient to context and save changes.
            // 5. Map to PatientResponseDTO and return.

            var account = await _context.Accounts.FindAsync(patientRequest.AccId);
            if (account == null)
                throw new ArgumentException("Account does not exist.");
            if (account.Roles != 3) // 3 = Patient
                throw new ArgumentException("Account must have Patient role (Roles = 3).");
            if (await _context.Patients.AnyAsync(p => p.AccId == patientRequest.AccId))
                throw new InvalidOperationException("Patient with this AccId already exists.");

            var patient = new Patient
            {
                PtnId = patientRequest.AccId,
                AccId = patientRequest.AccId,
                Account = account
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return new PatientResponseDTO
            {
                PtnId = patient.PtnId,
                AccId = patient.AccId,
                Account = new AccountResponseDTO
                {
                    AccId = account.AccId,
                    AccUsername = account.AccUsername,
                    AccPassword = account.AccPassword,
                    Email = account.Email,
                    Fullname = account.Fullname,
                    Dob = account.Dob,
                    Gender = account.Gender,
                    Roles = account.Roles,
                    IsActive = account.IsActive
                }
            };
        }

        public async Task<PatientResponseDTO> UpdatePatientAsync(int patientId, PatientRequestDTO patientRequest)
        {
            // 1. Find the Patient by patientId, including Account.
            var patient = await _context.Patients
                .Include(p => p.Account)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (patient == null)
                return null;

            // 2. Validate AccId
            var account = await _context.Accounts.FindAsync(patientRequest.AccId);
            if (account == null)
                throw new ArgumentException("Account does not exist.");
            if (account.Roles != 3)
                throw new ArgumentException("Account must have Patient role (Roles = 3).");
            if (await _context.Patients.AnyAsync(p => p.AccId == patientRequest.AccId && p.PtnId != patientId))
                throw new InvalidOperationException("Patient with this AccId already exists.");

            // 3. Update fields
            patient.AccId = patientRequest.AccId;

            await _context.SaveChangesAsync();

            return new PatientResponseDTO
            {
                PtnId = patient.PtnId,
                AccId = patient.AccId,
                Account = new AccountResponseDTO
                {
                    AccId = account.AccId,
                    AccUsername = account.AccUsername,
                    AccPassword = account.AccPassword,
                    Email = account.Email,
                    Fullname = account.Fullname,
                    Dob = account.Dob,
                    Gender = account.Gender,
                    Roles = account.Roles,
                    IsActive = account.IsActive
                }
            };
        }
    }
}
