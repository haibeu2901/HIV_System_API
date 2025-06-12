using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientDTO;
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

        public async Task<PatientResponseDTO> CreatePatientAsync(PatientRequestDTO patientRequest)
        {
            return await _patientRepo.CreatePatientAsync(patientRequest);
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            return await _patientRepo.DeletePatientAsync(patientId);
        }

        public async Task<List<PatientResponseDTO>> GetAllPatientsAsync()
        {
            return await _patientRepo.GetAllPatientsAsync();
        }

        public async Task<PatientResponseDTO> GetPatientByIdAsync(int patientId)
        {
            return await _patientRepo.GetPatientByIdAsync(patientId);
        }

        public async Task<PatientResponseDTO> UpdatePatientAsync(int patientId, PatientRequestDTO patientRequest)
        {
            return await _patientRepo.UpdatePatientAsync(patientId, patientRequest);
        }
    }
}
