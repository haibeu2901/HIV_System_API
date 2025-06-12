using HIV_System_API_BOs;
using HIV_System_API_DTOs;
using HIV_System_API_DTOs.PatientDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IPatientRepo
    {
        Task<List<PatientResponseDTO>> GetAllPatientsAsync();
        Task<PatientResponseDTO> GetPatientByIdAsync(int patientId);
        Task<bool> DeletePatientAsync(int patientId);
        Task<PatientResponseDTO> CreatePatientAsync(PatientRequestDTO patientRequest);
        Task<PatientResponseDTO> UpdatePatientAsync(int patientId, PatientRequestDTO patientRequest);
    }
}
