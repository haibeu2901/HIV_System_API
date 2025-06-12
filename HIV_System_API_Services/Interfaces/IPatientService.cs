using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IPatientService
    {
        Task<List<PatientDTO>> GetAllPatientsAsync();
        Task<PatientDTO> GetPatientByIdAsync(int patientId);
        Task<bool> DeletePatientAsync(int patientId);
        Task<PatientDTO> CreatePatientAsync(int accId);
        Task<bool> UpdatePatientAsync(int patientId, PatientDTO updatedPatient);
    }
}
