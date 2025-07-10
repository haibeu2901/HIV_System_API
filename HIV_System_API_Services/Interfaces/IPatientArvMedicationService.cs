using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientArvMedicationDTO;

namespace HIV_System_API_Services.Interfaces
{
    public interface IPatientArvMedicationService
    {
        Task<List<PatientArvMedicationResponseDTO>> GetAllPatientArvMedicationsAsync();
        Task<PatientArvMedicationResponseDTO?> GetPatientArvMedicationByIdAsync(int pamId);
        Task<PatientArvMedicationResponseDTO> CreatePatientArvMedicationAsync(PatientArvMedicationRequestDTO patientArvMedication);
        Task<PatientArvMedicationResponseDTO> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedicationRequestDTO patientArvMedication);
        Task<bool> DeletePatientArvMedicationAsync(int pamId);
        Task<List<PatientArvMedicationResponseDTO>> GetPatientArvMedicationsByPatientIdAsync(int patientId);
        Task<List<PatientArvMedicationResponseDTO>> GetPatientArvMedicationsByPatientRegimenIdAsync(int parId);
    }
}