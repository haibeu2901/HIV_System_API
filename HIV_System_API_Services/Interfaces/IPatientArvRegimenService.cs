using HIV_System_API_DTOs.PatientARVRegimenDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IPatientArvRegimenService
    {
        Task<List<PatientArvRegimenResponseDTO>> GetAllPatientArvRegimensAsync();
        Task<PatientArvRegimenResponseDTO?> GetPatientArvRegimenByIdAsync(int parId);
        Task<PatientArvRegimenResponseDTO> CreatePatientArvRegimenAsync(PatientArvRegimenRequestDTO patientArvRegimen);
        Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimenRequestDTO patientArvRegimen);
        Task<bool> DeletePatientArvRegimenAsync(int parId);
    }
}
