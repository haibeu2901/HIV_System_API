using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
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

        private Patient MapToEntity(PatientRequestDTO patientRequest)
        {
            return new Patient
            {
                PtnId = patientRequest.AccId, // Set PtnId to match AccId
                AccId = patientRequest.AccId
            };
        }

        private PatientResponseDTO MapToResponseDTO(Patient patient)
        {
            if (patient == null || patient.Account == null)
                return null!;

            return new PatientResponseDTO
            {
                PtnId = patient.PtnId,
                AccId = patient.AccId,
                Account = new AccountResponseDTO
                {
                    AccId = patient.Account.AccId,
                    AccUsername = patient.Account.AccUsername,
                    AccPassword = patient.Account.AccPassword,
                    Email = patient.Account.Email,
                    Fullname = patient.Account.Fullname,
                    Dob = patient.Account.Dob,
                    Gender = patient.Account.Gender,
                    Roles = patient.Account.Roles,
                    IsActive = patient.Account.IsActive
                }
            };
        }

        public async Task<PatientResponseDTO> CreatePatientAsync(PatientRequestDTO patient)
        {
            // Map DTO to entity
            var patientEntity = MapToEntity(patient);

            // Create patient in repository
            var createdPatient = await _patientRepo.CreatePatientAsync(patientEntity);

            // Map entity to response DTO
            var responseDto = MapToResponseDTO(createdPatient);

            return responseDto;
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            // Call the repository to delete the patient by ID
            return await _patientRepo.DeletePatientAsync(patientId);
        }

        public async Task<List<PatientResponseDTO>> GetAllPatientsAsync()
        {
            // Retrieve all patients from the repository
            var patients = await _patientRepo.GetAllPatientsAsync();

            // Map each patient entity to a PatientResponseDTO
            var responseDtos = patients
                .Select(MapToResponseDTO)
                .ToList();

            return responseDtos;
        }

        public async Task<PatientResponseDTO?> GetPatientByIdAsync(int patientId)
        {
            // Retrieve the patient entity from the repository
            var patient = await _patientRepo.GetPatientByIdAsync(patientId);

            // Map the entity to a response DTO (returns null if patient is null)
            var responseDto = MapToResponseDTO(patient);

            return responseDto;
        }

        public async Task<PatientResponseDTO> UpdatePatientAsync(int patientId, PatientRequestDTO patient)
        {
            // Retrieve the existing patient entity
            var existingPatient = await _patientRepo.GetPatientByIdAsync(patientId);
            if (existingPatient == null)
            {
                return null!;
            }

            // Update properties (currently only AccId is available in PatientRequestDTO)
            existingPatient.AccId = patient.AccId;

            // Update patient in repository
            var updatedPatient = await _patientRepo.UpdatePatientAsync(patientId, existingPatient);

            // Map updated entity to response DTO
            var responseDto = MapToResponseDTO(updatedPatient);

            return responseDto;
        }
    }
}
