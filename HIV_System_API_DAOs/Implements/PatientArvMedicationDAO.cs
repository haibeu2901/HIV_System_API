using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_DAOs.Implements
{
    public class PatientArvMedicationDAO : IPatientArvMedicationDAO
    {
        private readonly HivSystemApiContext _context;
        private static PatientArvMedicationDAO? _instance;

        public static PatientArvMedicationDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PatientArvMedicationDAO();
                }
                return _instance;
            }
        }

        public PatientArvMedicationDAO()
        {
            _context = new HivSystemApiContext();
        }

        /// <summary>
        /// Get all patient ARV medications with navigation properties
        /// </summary>
        public async Task<List<PatientArvMedication>> GetAllPatientArvMedicationsAsync()
        {
            return await _context.PatientArvMedications
                .Include(pam => pam.Amd) // Include ARV Medication Detail
                .Include(pam => pam.Par) // Include Patient ARV Regimen
                .OrderByDescending(pam => pam.Par.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get patient ARV medication by ID with navigation properties
        /// </summary>
        public async Task<PatientArvMedication?> GetPatientArvMedicationByIdAsync(int pamId)
        {
            return await _context.PatientArvMedications
                .Include(pam => pam.Amd)
                .Include(pam => pam.Par)
                .FirstOrDefaultAsync(pam => pam.PamId == pamId);
        }

        /// <summary>
        /// Create new patient ARV medication
        /// </summary>
        public async Task<PatientArvMedication> CreatePatientArvMedicationAsync(PatientArvMedication patientArvMedication)
        {
            if (patientArvMedication == null)
                throw new ArgumentNullException(nameof(patientArvMedication));

            await _context.PatientArvMedications.AddAsync(patientArvMedication);
            await _context.SaveChangesAsync();

            // Return the created entity with navigation properties loaded
            return await GetPatientArvMedicationByIdAsync(patientArvMedication.PamId)
                ?? throw new InvalidOperationException("Failed to retrieve created medication");
        }

        /// <summary>
        /// Update existing patient ARV medication
        /// </summary>
        public async Task<PatientArvMedication> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedication patientArvMedication)
        {
            if (patientArvMedication == null)
                throw new ArgumentNullException(nameof(patientArvMedication));

            var existingMedication = await _context.PatientArvMedications.FindAsync(pamId);
            if (existingMedication == null)
                throw new KeyNotFoundException($"No ARV medication found with ID {pamId}.");

            // Update properties
            existingMedication.ParId = patientArvMedication.ParId;
            existingMedication.AmdId = patientArvMedication.AmdId;
            existingMedication.Quantity = patientArvMedication.Quantity;

            _context.PatientArvMedications.Update(existingMedication);
            await _context.SaveChangesAsync();

            // Return updated entity with navigation properties
            return await GetPatientArvMedicationByIdAsync(pamId)
                ?? throw new InvalidOperationException("Failed to retrieve updated medication");
        }

        /// <summary>
        /// Delete patient ARV medication
        /// </summary>
        public async Task<bool> DeletePatientArvMedicationAsync(int pamId)
        {
            var medication = await _context.PatientArvMedications.FindAsync(pamId);
            if (medication == null)
                return false;

            _context.PatientArvMedications.Remove(medication);
            await _context.SaveChangesAsync();
            return true;
        }


        /// <summary>
        /// Get all medications for a specific patient ARV regimen
        /// </summary>
        public async Task<List<PatientArvMedication>> GetPatientArvMedicationsByPatientRegimenIdAsync(int parId)
        {
            return await _context.PatientArvMedications
                .Include(pam => pam.Amd)
                .Include(pam => pam.Par)
                .Where(pam => pam.ParId == parId)
                .OrderBy(pam => pam.Amd.MedName)
                .OrderByDescending(pam => pam.Par.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Get all medications for a specific patient (across all regimens)
        /// </summary>
        public async Task<List<PatientArvMedication>> GetPatientArvMedicationsByPatientIdAsync(int patientId)
        {
            return await _context.PatientArvMedications
                .Include(pam => pam.Amd)
                .Include(pam => pam.Par)
                    .ThenInclude(par => par.Pmr)
                    .ThenInclude(pmr => pmr.Ptn)
                .Where(pam => pam.Par.Pmr.PtnId == patientId)
                .OrderBy(pam => pam.Par.CreatedAt)
                .ThenBy(pam => pam.Amd.MedName)
                .ToListAsync();
        }
    }
}