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
        private static PatientArvRegimenDAO _instance;
        private readonly HivSystemApiContext _context;

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

        public async Task<List<PatientArvRegimen>> GetAllPatientArvRegimensAsync()
        {
            return await _context.PatientArvRegimen.ToListAsync();
        }

        public async Task<PatientArvRegimen?> GetPatientArvRegimenByIdAsync(int parId)
        {
            return await _context.PatientArvRegimen.FindAsync(parId);
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
            existingRegimen.StartDate = patientArvRegimen.StartDate;
            existingRegimen.EndDate = patientArvRegimen.EndDate;
            existingRegimen.Notes = patientArvRegimen.Notes;
            existingRegimen.RegimenLevel = patientArvRegimen.RegimenLevel;
            existingRegimen.RegimenStatus = patientArvRegimen.RegimenStatus;
            existingRegimen.TotalCost = patientArvRegimen.TotalCost;

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
    }
}
