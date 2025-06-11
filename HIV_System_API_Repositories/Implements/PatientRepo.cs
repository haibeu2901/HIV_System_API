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
    public class PatientRepo : IPatientRepo
    {
        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await PatientDAO.Instance.GetAllPatientsAsync();
        }

        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
            return await PatientDAO.Instance.GetPatientByIdAsync(patientId);
        }

        public async Task<List<Patient>> GetPatientsByNameAsync(string name)
        {
            return await PatientDAO.Instance.GetPatientsByNameAsync(name);
        }
    }
}
