using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_DAOs.Implements
{
    public class PatientArvMedicationDAO : IPatientArvMedicationDAO
    {
        private static PatientArvMedicationDAO? _instance;
        private readonly HivSystemContext _context;

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
            _context = new HivSystemContext();
        }

        public async Task<List<PatientArvMedication>> GetAllPatientArvMedicationsAsync()
        {
            return await _context.PatientArvMedications.ToListAsync();
        }

        public async Task<PatientArvMedication?> GetPatientArvMedicationByIdAsync(int pamId)
        {
            return await _context.PatientArvMedications.FindAsync(pamId);
        }

        public async Task<PatientArvMedication> CreatePatientArvMedicationAsync(PatientArvMedication patientArvMedication)
        {
            if (patientArvMedication == null)
                throw new ArgumentNullException(nameof(patientArvMedication));

            await _context.PatientArvMedications.AddAsync(patientArvMedication);
            await _context.SaveChangesAsync();
            return patientArvMedication;
        }

        public async Task<PatientArvMedication> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedication patientArvMedication)
        {
            if (patientArvMedication == null)
                throw new ArgumentNullException(nameof(patientArvMedication));

            var existingMedication = await _context.PatientArvMedications.FindAsync(pamId);
            if (existingMedication == null)
                throw new KeyNotFoundException($"No ARV medication found with ID {pamId}.");

            existingMedication.ParId = patientArvMedication.ParId;
            existingMedication.AmdId = patientArvMedication.AmdId;
            existingMedication.Quantity = patientArvMedication.Quantity;

            _context.PatientArvMedications.Update(existingMedication);
            await _context.SaveChangesAsync();
            return existingMedication;
        }

        public async Task<bool> DeletePatientArvMedicationAsync(int pamId)
        {
            var medication = await _context.PatientArvMedications.FindAsync(pamId);
            if (medication == null)
                return false;

            _context.PatientArvMedications.Remove(medication);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}