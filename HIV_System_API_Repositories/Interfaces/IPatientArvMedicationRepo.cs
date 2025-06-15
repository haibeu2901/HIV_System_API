using HIV_System_API_BOs;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IPatientArvMedicationRepo
    {
        Task<List<PatientArvMedication>> GetAllPatientArvMedicationsAsync();
        Task<PatientArvMedication?> GetPatientArvMedicationByIdAsync(int pamId);
        Task<PatientArvMedication> CreatePatientArvMedicationAsync(PatientArvMedication patientArvMedication);
        Task<PatientArvMedication> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedication patientArvMedication);
        Task<bool> DeletePatientArvMedicationAsync(int pamId);
    }
}