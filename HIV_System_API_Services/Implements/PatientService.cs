using HIV_System_API_BOs;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepo _patientRepo;

        public PatientService()
        {
            _patientRepo = new PatientRepo();
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _patientRepo.GetAllPatientsAsync();
        }

        public async Task<Patient> GetPatientByIdAsync(int patientId)
        {
            return await _patientRepo.GetPatientByIdAsync(patientId);
        }

        public async Task<List<Patient>> GetPatientsByNameAsync(string name)
        {
            return await _patientRepo.GetPatientsByNameAsync(name);
        }
    }
}
