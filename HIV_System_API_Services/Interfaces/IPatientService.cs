using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientResponseDTO>> GetAllPatientsAsync();
        Task<PatientResponseDTO?> GetPatientByIdAsync(int patientId);
        Task<PatientMRResponseDTO?> GetPatientMRByIdAsync(int patientId);
        Task<bool> DeletePatientAsync(int patientId);
        Task<PatientResponseDTO?> CreatePatientAsync(PatientRequestDTO patient);
        Task<PatientResponseDTO> UpdatePatientAsync(int patientId, PatientRequestDTO patient);
    }
}
