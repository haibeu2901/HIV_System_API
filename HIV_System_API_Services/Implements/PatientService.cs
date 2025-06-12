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

        private PatientDTO ToDTO(Patient patient)
        {
            if (patient == null) return null;

            return new PatientDTO
            {
                PtnId = patient.PtnId,
                AccId = patient.AccId,
                AccUsername = patient.Account?.AccUsername,
                AccPassword = patient.Account?.AccPassword,
                AccEmail = patient.Account?.Email,
                FullName = patient.Account?.Fullname,
                Dob = patient.Account?.Dob.HasValue == true ? patient.Account.Dob.Value.ToDateTime(TimeOnly.MinValue) : null,
                Gender = patient.Account?.Gender
            };
        }

        private Patient ToEntity(PatientDTO patientDto)
        {
            if (patientDto == null) return null;

            var account = new Account
            {
                AccId = patientDto.AccId,
                AccUsername = patientDto.AccUsername,
                AccPassword = patientDto.AccPassword,
                Email = patientDto.AccEmail,
                Fullname = patientDto.FullName,
                Dob = patientDto.Dob.HasValue ? DateOnly.FromDateTime(patientDto.Dob.Value) : null,
                Gender = patientDto.Gender
            };

            return new Patient
            {
                PtnId = patientDto.PtnId,
                AccId = patientDto.AccId,
                Account = account
            };
        }

        public async Task<PatientDTO> CreatePatientAsync(int accId)
        {
            if (accId <= 0)
                throw new ArgumentException("Invalid account ID.", nameof(accId));

            // Create a new Patient entity with the given accId
            var patient = new Patient
            {
                AccId = accId
            };

            // Call repository to create the patient
            var createdPatient = await _patientRepo.CreatePatientAsync(accId);

            // Convert to DTO and return
            return ToDTO(createdPatient);
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            if (patientId <= 0)
                throw new ArgumentException("Invalid patient ID.", nameof(patientId));

            return await _patientRepo.DeletePatientAsync(patientId);
        }

        public async Task<List<PatientDTO>> GetAllPatientsAsync()
        {
            var patients = await _patientRepo.GetAllPatientsAsync();
            if (patients == null || patients.Count == 0)
                return new List<PatientDTO>();

            return patients.Select(ToDTO).ToList();
        }

        public async Task<PatientDTO> GetPatientByIdAsync(int patientId)
        {
            if (patientId <= 0)
                throw new ArgumentException("Invalid patient ID.", nameof(patientId));

            var patient = await _patientRepo.GetPatientByIdAsync(patientId);
            return ToDTO(patient);
        }

        public async Task<bool> UpdatePatientAsync(int patientId, PatientDTO updatedPatient)
        {
            if (patientId <= 0)
                throw new ArgumentException("Invalid patient ID.", nameof(patientId));
            if (updatedPatient == null)
                throw new ArgumentNullException(nameof(updatedPatient));

            // Convert DTO to entity
            var patientEntity = ToEntity(updatedPatient);

            // Call repository to update patient
            return await _patientRepo.UpdatePatientAsync(patientId, patientEntity);
        }

    }
}
