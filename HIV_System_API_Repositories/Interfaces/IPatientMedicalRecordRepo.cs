using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IPatientMedicalRecordRepo
    {
        Task<List<PatientMedicalRecord>> GetAllPatientMedicalRecordsAsync();
        Task<PatientMedicalRecord?> GetPatientMedicalRecordByIdAsync(int id);
        Task<PatientMedicalRecord> CreatePatientMedicalRecordAsync(PatientMedicalRecord record);
        Task<PatientMedicalRecord> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecord record);
        Task<bool> DeletePatientMedicalRecordAsync(int id);
        Task<PatientMedicalRecord?> GetPersonalMedicalRecordAsync(int accId);
    }
}
