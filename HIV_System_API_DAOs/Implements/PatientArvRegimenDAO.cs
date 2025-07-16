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
    public class PatientArvRegimenDAO : IPatientArvRegimenDAO
    {
        private readonly HivSystemApiContext _context;
        private static PatientArvRegimenDAO? _instance;

        public static PatientArvRegimenDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PatientArvRegimenDAO();
                }
                return _instance;
            }
        }

        public PatientArvRegimenDAO()
        {
            _context = new HivSystemApiContext();
        }

        // Use for unit testing or dependency injection
        public PatientArvRegimenDAO(HivSystemApiContext context)
        {
            _context = context;
        }

        public async Task<List<PatientArvRegimen>> GetAllPatientArvRegimensAsync()
        {
            return await _context.PatientArvRegimen
                .Include(p => p.Pmr)
                .Include(p => p.PatientArvMedications)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PatientArvRegimen?> GetPatientArvRegimenByIdAsync(int parId)
        {
            return await _context.PatientArvRegimen
                .Include(p => p.Pmr)
                .Include(p => p.PatientArvMedications)
                .SingleOrDefaultAsync(p => p.ParId == parId);
        }

        public async Task<PatientArvRegimen> CreatePatientArvRegimenAsync(PatientArvRegimen patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen), "Patient ARV regimen cannot be null.");

            await _context.PatientArvRegimen.AddAsync(patientArvRegimen);
            await _context.SaveChangesAsync();
            return patientArvRegimen;
        }

        public async Task<PatientArvRegimen> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimen patientArvRegimen)
        {
            if (patientArvRegimen == null)
            {
                throw new ArgumentNullException(nameof(patientArvRegimen), "Patient ARV regimen cannot be null.");
            }

            var existingRegimen = await _context.PatientArvRegimen.FindAsync(parId);
            if (existingRegimen == null)
            {
                throw new KeyNotFoundException($"No ARV regimen found with ID {parId}.");
            }

            // Update all properties
            existingRegimen.PmrId = patientArvRegimen.PmrId; // Allow PMR ID updates
            existingRegimen.StartDate = patientArvRegimen.StartDate;
            existingRegimen.EndDate = patientArvRegimen.EndDate;
            existingRegimen.Notes = patientArvRegimen.Notes;
            existingRegimen.RegimenLevel = patientArvRegimen.RegimenLevel;
            existingRegimen.RegimenStatus = patientArvRegimen.RegimenStatus;
            existingRegimen.TotalCost = patientArvRegimen.TotalCost;
            existingRegimen.CreatedAt = patientArvRegimen.CreatedAt;

            _context.PatientArvRegimen.Update(existingRegimen);
            await _context.SaveChangesAsync();
            return existingRegimen;
        }

        public async Task<bool> DeletePatientArvRegimenAsync(int parId)
        {
            var regimen = await _context.PatientArvRegimen.FindAsync(parId);
            if (regimen == null)
            {
                return false; // Regimen not found
            }

            _context.PatientArvRegimen.Remove(regimen);
            await _context.SaveChangesAsync();
            return true; // Deletion successful
        }

        public async Task<List<PatientArvRegimen>> GetPatientArvRegimensByPatientIdAsync(int patientId)
        {
            return await _context.PatientArvRegimen
                .Include(p => p.Pmr)
                .Include(p => p.PatientArvMedications)
                .Where(p => p.Pmr.PtnId == patientId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<PatientArvRegimen>> GetPersonalArvRegimensAsync(int personalId)
        {
            return await _context.PatientArvRegimen
                .Include(p => p.Pmr)
                .Include(p => p.PatientArvMedications)
                .Where(p => p.Pmr.PtnId == personalId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PatientArvRegimen> InitiatePatientArvRegimenAsync(int patientId)
        {
            // First, verify the patient exists by finding their PMR
            var pmr = await _context.PatientMedicalRecords
                .FirstOrDefaultAsync(p => p.PtnId == patientId);

            if (pmr == null)
            {
                throw new KeyNotFoundException($"No patient found with ID {patientId}.");
            }

            // Create an empty ARV regimen placeholder - only PMR ID is set
            var newRegimen = new PatientArvRegimen
            {
                PmrId = pmr.PmrId
                // All other properties (StartDate, EndDate, Notes, RegimenLevel, RegimenStatus, TotalCost) 
                // will be null/default and filled later when medications are added
                // CreatedAt will be set automatically by database default
            };

            await _context.PatientArvRegimen.AddAsync(newRegimen);
            await _context.SaveChangesAsync();

            // Set the navigation property manually and return the new regimen
            newRegimen.Pmr = pmr;
            newRegimen.PatientArvMedications = new List<PatientArvMedication>(); // Empty collection

            return newRegimen;
        }
        public async Task<PatientArvRegimen> UpdatePatientArvRegimenStatusAsync(int parId, byte status, string? notes = null)
        {
            var regimen = await _context.PatientArvRegimen.FindAsync(parId);
            if (regimen == null)
            {
                throw new KeyNotFoundException($"No ARV regimen found with ID {parId}.");
            }
            // Update the regimen status and notes
            regimen.RegimenStatus = status;
            regimen.Notes = notes;
            _context.PatientArvRegimen.Update(regimen);
            await _context.SaveChangesAsync();
            return regimen;
        }
    }
}
