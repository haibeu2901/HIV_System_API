using HIV_System_API_BOs;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IPatientArvMedicationDAO
    {
        Task<List<PatientArvMedication>> GetAllPatientArvMedicationsAsync();
        Task<PatientArvMedication?> GetPatientArvMedicationByIdAsync(int pamId);
        Task<PatientArvMedication> CreatePatientArvMedicationAsync(PatientArvMedication patientArvMedication);
        Task<PatientArvMedication> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedication patientArvMedication);
        Task<bool> DeletePatientArvMedicationAsync(int pamId);
        Task<List<PatientArvMedication>> GetPatientArvMedicationsByPatientIdAsync(int patientId);
        Task<List<PatientArvMedication>> GetPatientArvMedicationsByPatientRegimenIdAsync(int parId);
    }
}