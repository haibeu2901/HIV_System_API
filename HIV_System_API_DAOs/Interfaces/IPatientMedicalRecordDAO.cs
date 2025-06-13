using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IPatientMedicalRecordDAO
    {
        Task<List<PatientMedicalRecord>> GetAllPatientMedicalRecordsAsync();
        Task<PatientMedicalRecord?> GetPatientMedicalRecordByIdAsync(int id);
        Task<PatientMedicalRecord> CreatePatientMedicalRecordAsync(PatientMedicalRecord record);
        Task<PatientMedicalRecord> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecord record);
        Task<bool> DeletePatientMedicalRecordAsync(int id);
    }
}
