using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IPatientArvRegimenRepo
    {
        Task<List<PatientArvRegimen>> GetAllPatientArvRegimensAsync();
        Task<PatientArvRegimen?> GetPatientArvRegimenByIdAsync(int parId);
        Task<PatientArvRegimen> CreatePatientArvRegimenAsync(PatientArvRegimen patientArvRegimen);
        Task<PatientArvRegimen> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimen patientArvRegimen);
        Task<bool> DeletePatientArvRegimenAsync(int parId);
    }
}
