using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.Appointment;
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
        private readonly HivSystemApiContext _context;

        public PatientMedicalRecordService()
        {
            _patientMedicalRecordRepo = new PatientMedicalRecordRepo();
            _context = new HivSystemApiContext();
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
            // Get all appointments for the patient
            var appointments = _context.Appointments
                .Where(a => a.PtnId == record.PtnId)
                .Select(a => new AppointmentResponseDTO
                {
                    ApmId = a.ApmId,
                    PtnId = a.PtnId,
                    PatientName = a.Ptn.Acc.Fullname,
                    DctId = a.DctId,
                    DoctorName = a.Dct.Acc.Fullname,
                    ApmtDate = a.ApmtDate,
                    ApmTime = a.ApmTime,
                    Notes = a.Notes,
                    ApmStatus = a.ApmStatus
                })
                .ToList();

            return new PatientMedicalRecordResponseDTO
            {
                PmrId = record.PmrId,
                PtnId = record.PtnId,
                Appointments = appointments
            };
        }

        public async Task<PatientMedicalRecordResponseDTO> CreatePatientMedicalRecordAsync(PatientMedicalRecordRequestDTO record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            // Validation: PtnId must be positive
            if (record.PtnId <= 0)
                throw new ArgumentException("Patient ID (PtnId) must be a positive integer.", nameof(record.PtnId));

            // Optionally, check if a record for this patient already exists (if business logic requires)
            // var existing = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(record.PtnId);
            // if (existing != null)
            //     throw new InvalidOperationException($"A medical record for patient ID {record.PtnId} already exists.");

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

            // Validation: PtnId must be positive
            if (record.PtnId <= 0)
                throw new ArgumentException("Patient ID (PtnId) must be a positive integer.", nameof(record.PtnId));

            // Retrieve the existing entity
            var existingRecord = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(id);
            if (existingRecord == null)
                throw new KeyNotFoundException($"PatientMedicalRecord with id {id} not found.");

            // Optionally, prevent changing to a PtnId that already has a record (if business logic requires)
            // var otherRecord = await _patientMedicalRecordRepo.GetPatientMedicalRecordByIdAsync(record.PtnId);
            // if (otherRecord != null && otherRecord.PmrId != id)
            //     throw new InvalidOperationException($"A medical record for patient ID {record.PtnId} already exists.");

            // Update properties
            existingRecord.PtnId = record.PtnId;

            // Update in repository
            var updatedEntity = await _patientMedicalRecordRepo.UpdatePatientMedicalRecordAsync(id, existingRecord);
            return MapToResponseDTO(updatedEntity);
        }

        public async Task<PatientMedicalRecordResponseDTO?> GetPersonalMedicalRecordAsync(int accId)
        {
            // Validation: patientId must be positive
            if (accId <= 0)
                throw new ArgumentException("Account ID (accId) must be a positive integer.", nameof(accId));

            // Retrieve the personal medical record from the repository
            var record = await _patientMedicalRecordRepo.GetPersonalMedicalRecordAsync(accId);
            if (record == null)
                return null;

            return MapToResponseDTO(record);
        }
    }
}
