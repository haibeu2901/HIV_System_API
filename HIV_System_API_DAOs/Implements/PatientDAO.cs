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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class PatientDAO : IPatientDAO
    {
        private readonly HivSystemApiContext _context;
        private static PatientDAO? _instance;

        public PatientDAO()
        {
            _context = new HivSystemApiContext();
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
                .Include(p => p.Acc)
                .Include(p => p.PatientMedicalRecord)
                .ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.Acc)
                .Include(p => p.PatientMedicalRecord)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            Debug.WriteLine($"Attempting to delete patient with PtnId: {patientId}");
            var patient = await _context.Patients
                .Include(p => p.PatientMedicalRecord)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (patient == null)
            {
                Debug.WriteLine($"Patient with PtnId: {patientId} not found.");
                return false; // Return false if the patient does not exist
            }

            // Delete PatientMedicalRecord first if exists
            if (patient.PatientMedicalRecord != null)
            {
                _context.PatientMedicalRecords.Remove(patient.PatientMedicalRecord);
                await _context.SaveChangesAsync();
                // Reload patient to ensure it's still tracked and not deleted by cascade
                patient = await _context.Patients.FirstOrDefaultAsync(p => p.PtnId == patientId);
                if (patient == null)
                {
                    Debug.WriteLine($"Patient with PtnId: {patientId} was deleted by cascade after removing PatientMedicalRecord.");
                    return true;
                }
            }

            try
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
                Debug.WriteLine($"Successfully deleted patient with PtnId: {patientId}");
                return true;
            }
            catch (DbUpdateException ex)
            {
                Debug.WriteLine($"Failed to delete patient with PtnId: {patientId}. Error: {ex.Message}, InnerException: {ex.InnerException?.Message}");
                throw new InvalidOperationException($"Failed to delete patient: {ex.Message}", ex);
            }
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            if (patient == null)
                throw new ArgumentNullException(nameof(patient));

            // Attach Acc if not tracked
            if (patient.Acc != null)
            {
                _context.Accounts.Attach(patient.Acc);
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
                .Include(p => p.Acc)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (existingPatient == null)
                throw new KeyNotFoundException($"Patient with ID {patientId} not found.");

            // Update Patient properties
            existingPatient.AccId = patient.AccId;

            // Update Acc if provided
            if (patient.Acc != null && existingPatient.Acc != null)
            {
                existingPatient.Acc.AccUsername = patient.Acc.AccUsername;
                existingPatient.Acc.AccPassword = patient.Acc.AccPassword;
                existingPatient.Acc.Email = patient.Acc.Email;
                existingPatient.Acc.Fullname = patient.Acc.Fullname;
                existingPatient.Acc.Dob = patient.Acc.Dob;
                existingPatient.Acc.Gender = patient.Acc.Gender;
                existingPatient.Acc.Roles = patient.Acc.Roles;
                existingPatient.Acc.IsActive = patient.Acc.IsActive;
            }

            // Update PatientMedicalRecord if needed (not shown here)

            await _context.SaveChangesAsync();
            return existingPatient;
        }
    }
}
