using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class PatientMedicalRecordRepo : IPatientMedicalRecordRepo
    {
        public async Task<PatientMedicalRecord> CreatePatientMedicalRecordAsync(PatientMedicalRecord record)
        {
            return await PatientMedicalRecordDAO.Instance.CreatePatientMedicalRecordAsync(record);
        }

        public async Task<bool> DeletePatientMedicalRecordAsync(int id)
        {
            return await PatientMedicalRecordDAO.Instance.DeletePatientMedicalRecordAsync(id);
        }

        public async Task<List<PatientMedicalRecord>> GetAllPatientMedicalRecordsAsync()
        {
            return await PatientMedicalRecordDAO.Instance.GetAllPatientMedicalRecordsAsync();
        }

        public async Task<PatientMedicalRecord?> GetPatientMedicalRecordByIdAsync(int id)
        {
            return await PatientMedicalRecordDAO.Instance.GetPatientMedicalRecordByIdAsync(id);
        }

        public async Task<PatientMedicalRecord> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecord record)
        {
            return await PatientMedicalRecordDAO.Instance.UpdatePatientMedicalRecordAsync(id, record);
        }
    }
}
