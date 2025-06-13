using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
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
    public class PatientMedicalRecordService : IPatientMedicalRecordService
    {
        private readonly IPatientMedicalRecordRepo _patientMedicalRecordRepo;

        public PatientMedicalRecordService()
        {
            _patientMedicalRecordRepo = new PatientMedicalRecordRepo();
        }

        private PatientMedicalRecord MapToEntity(PatientMedicalRecordRequestDTO requestDTO)
        {
            return new PatientMedicalRecord
            {
                PtnId = requestDTO.PtnId
                // Collections (Appointments, PatientArvRegimen, TestResults) and navigation properties (Ptn) are not set here
                // as they are typically managed by the ORM or set elsewhere
            };
        }

        private PatientMedicalRecordResponseDTO MapToResponseDTO(PatientMedicalRecord record)
        {
            return new PatientMedicalRecordResponseDTO
            {
                PmrId = record.PmrId,
                PtnId = record.PtnId
            };
        }

        public async Task<PatientMedicalRecordResponseDTO> CreatePatientMedicalRecordAsync(PatientMedicalRecordRequestDTO record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var entity = MapToEntity(record);
            var createdEntity = await _patientMedicalRecordRepo.CreatePatientMedicalRecordAsync(entity);
            return MapToResponseDTO(createdEntity);
        }

        public async Task<bool> DeletePatientMedicalRecordAsync(int id)
        {
            return await _patientMedicalRecordRepo.DeletePatientMedicalRecordAsync(id);
        }

        public async Task<List<PatientMedicalRecordResponseDTO>> GetAllPatientMedicalRecordsAsync()
        {
            var records = await _patientMedicalRecordRepo.GetAllPatientMedicalRecordsAsync();
            return records.Select(MapToResponseDTO).ToList();
        }

        public async Task<PatientMedicalRecordResponseDTO?> GetPatientMedicalRecordByIdAsync(int id)
        {
            var record = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(id);
            if (record == null)
                return null;
            return MapToResponseDTO(record);
        }

        public async Task<PatientMedicalRecordResponseDTO> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecordRequestDTO record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            // Retrieve the existing entity
            var existingRecord = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(id);
            if (existingRecord == null)
                throw new KeyNotFoundException($"PatientMedicalRecord with id {id} not found.");

            // Update properties
            existingRecord.PtnId = record.PtnId;

            // Update in repository
            var updatedEntity = await _patientMedicalRecordRepo.UpdatePatientMedicalRecordAsync(id, existingRecord);
            return MapToResponseDTO(updatedEntity);
        }
    }
}
