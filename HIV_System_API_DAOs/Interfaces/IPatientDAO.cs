using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IPatientDAO
    {
        Task<List<Patient>> GetAllPatientsAsync();
        Task<Patient> GetPatientByIdAsync(int patientId);
        Task<bool> DeletePatientAsync(int patientId);
        Task<Patient> CreatePatientAsync(int accId);
        Task<bool> UpdatePatientAsync(int patientId, Patient updatedPatient);
    }
}
