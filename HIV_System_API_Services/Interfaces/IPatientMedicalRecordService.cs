using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IPatientMedicalRecordService
    {
        Task<List<PatientMedicalRecordResponseDTO>> GetAllPatientMedicalRecordsAsync();
        Task<PatientMedicalRecordResponseDTO?> GetPatientMedicalRecordByIdAsync(int id);
        Task<PatientMedicalRecordResponseDTO> CreatePatientMedicalRecordAsync(PatientMedicalRecordRequestDTO record);
        Task<PatientMedicalRecordResponseDTO> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecordRequestDTO record);
        Task<bool> DeletePatientMedicalRecordAsync(int id);
        Task<PatientMedicalRecordResponseDTO?> GetPersonalMedicalRecordAsync(int accId);
        Task<PatientMedicalRecordResponseDTO?> GetPatientMedicalRecordByPatientIdAsync(int patientId);
    }
}
