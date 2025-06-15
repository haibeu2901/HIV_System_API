using HIV_System_API_BOs;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepo _patientRepo;
        private readonly IPatientMedicalRecordRepo _patientMedicalRecordRepo;
        private readonly HivSystemApiContext _context;

        public PatientService()
        {
            _context = new HivSystemApiContext();
            _patientRepo = new PatientRepo();
            _patientMedicalRecordRepo = new PatientMedicalRecordRepo();
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
            var account = _context.Accounts
           .FirstOrDefault(a => a.AccId == patient.AccId)
           ?? throw new InvalidOperationException("Associated account not found.");

            var medicalRecord = _context.PatientMedicalRecords
                .FirstOrDefault(r => r.PtnId == patient.PtnId);

            return new PatientResponseDTO
            {
                PatientId = patient.PtnId,
                AccId = patient.AccId,
                Account = new AccountResponseDTO
                {
                    AccId = account.AccId,
                    AccUsername = account.AccUsername,
                    Email = account.Email,
                    Fullname = account.Fullname,
                    Dob = account.Dob,
                    Gender = account.Gender,
                    Roles = account.Roles,
                    IsActive = account.IsActive
                }
            };
        }

        public async Task<PatientResponseDTO?> CreatePatientAsync(PatientRequestDTO patient)
        {
            // Map DTO to entity
            var patientEntity = MapToEntity(patient);

            // Create patient in repository
            var createdPatient = await _patientRepo.CreatePatientAsync(patientEntity);

            // Re-fetch patient from database to ensure PatientMedicalRecord is loaded and IDs are correct
            var patientWithRecord = await _patientRepo.GetPatientByIdAsync(createdPatient.PtnId);
            if(patientWithRecord == null)
            {
                return null;
            }
            // Map entity to response DTO
            var responseDto = MapToResponseDTO(patientWithRecord);

            return responseDto;
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            // Retrieve the patient entity
            var patient = await _patientRepo.GetPatientByIdAsync(patientId);
            if (patient == null)
            {
                return false;
            }

            // Delete associated PatientMedicalRecord if exists
            var medicalRecord = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(patientId);
            if (medicalRecord != null)
            {
                await _patientMedicalRecordRepo.DeletePatientMedicalRecordAsync(medicalRecord.PmrId);
            }

            // Delete the patient
            var result = await _patientRepo.DeletePatientAsync(patientId);
            return result;
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
            if (patient == null)
            {
                return null;
            }
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
