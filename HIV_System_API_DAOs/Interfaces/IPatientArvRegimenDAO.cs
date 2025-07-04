using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIV_System_API_BOs;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IPatientArvRegimenDAO
    {
        Task<List<PatientArvRegimen>> GetAllPatientArvRegimensAsync();
        Task<PatientArvRegimen?> GetPatientArvRegimenByIdAsync(int parId);
        Task<PatientArvRegimen> CreatePatientArvRegimenAsync(PatientArvRegimen patientArvRegimen);
        Task<PatientArvRegimen> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimen patientArvRegimen);
        Task<bool> DeletePatientArvRegimenAsync(int parId);
        Task<List<PatientArvRegimen>> GetPatientArvRegimensByPatientIdAsync(int patientId);
        Task<List<PatientArvRegimen>> GetPersonalArvRegimensAsync(int personalId);
    }
}
