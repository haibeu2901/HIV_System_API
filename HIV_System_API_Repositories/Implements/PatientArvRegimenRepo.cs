using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class PatientArvRegimenRepo : IPatientArvRegimenRepo
    {
        public async Task<PatientArvRegimen> CreatePatientArvRegimenAsync(PatientArvRegimen patientArvRegimen)
        {
            return await PatientArvRegimenDAO.Instance.CreatePatientArvRegimenAsync(patientArvRegimen);
        }

        public async Task<bool> DeletePatientArvRegimenAsync(int parId)
        {
            return await PatientArvRegimenDAO.Instance.DeletePatientArvRegimenAsync(parId);
        }

        public async Task<List<PatientArvRegimen>> GetAllPatientArvRegimensAsync()
        {
            return await PatientArvRegimenDAO.Instance.GetAllPatientArvRegimensAsync();
        }

        public async Task<PatientArvRegimen?> GetPatientArvRegimenByIdAsync(int parId)
        {
            return await PatientArvRegimenDAO.Instance.GetPatientArvRegimenByIdAsync(parId);
        }

        public async Task<List<PatientArvRegimen>> GetPatientArvRegimensByPatientIdAsync(int patientId)
        {
            return await PatientArvRegimenDAO.Instance.GetPatientArvRegimensByPatientIdAsync(patientId);
        }

        public async Task<PatientArvRegimen> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimen patientArvRegimen)
        {
            return await PatientArvRegimenDAO.Instance.UpdatePatientArvRegimenAsync(parId, patientArvRegimen);
        }
    }
}
