using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;

namespace HIV_System_API_Repositories.Implements
{
    public class PatientArvMedicationRepo : IPatientArvMedicationRepo
    {
        public async Task<List<PatientArvMedication>> GetAllPatientArvMedicationsAsync()
        {
            return await PatientArvMedicationDAO.Instance.GetAllPatientArvMedicationsAsync();
        }

        public async Task<PatientArvMedication?> GetPatientArvMedicationByIdAsync(int pamId)
        {
            return await PatientArvMedicationDAO.Instance.GetPatientArvMedicationByIdAsync(pamId);
        }

        public async Task<PatientArvMedication> CreatePatientArvMedicationAsync(PatientArvMedication patientArvMedication)
        {
            return await PatientArvMedicationDAO.Instance.CreatePatientArvMedicationAsync(patientArvMedication);
        }

        public async Task<PatientArvMedication> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedication patientArvMedication)
        {
            return await PatientArvMedicationDAO.Instance.UpdatePatientArvMedicationAsync(pamId, patientArvMedication);
        }

        public async Task<bool> DeletePatientArvMedicationAsync(int pamId)
        {
            return await PatientArvMedicationDAO.Instance.DeletePatientArvMedicationAsync(pamId);
        }
    }
}