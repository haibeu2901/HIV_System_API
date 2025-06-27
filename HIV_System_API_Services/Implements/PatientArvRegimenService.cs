using HIV_System_API_BOs;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PatientArvRegimenService : IPatientArvRegimenService
    {
        private readonly IPatientArvRegimenRepo _patientArvRegimenRepo;
        private readonly HivSystemApiContext _context; // Add this field

        public PatientArvRegimenService()
        {
            _patientArvRegimenRepo = new PatientArvRegimenRepo();
            _context = new HivSystemApiContext(); // Initialize context
        }

        private async Task ValidatePatientMedicalRecordExists(int pmrId)
        {
            var exists = await _context.PatientMedicalRecords.AnyAsync(pmr => pmr.PmrId == pmrId);

            if (!exists)
            {
                throw new InvalidOperationException($"Patient Medical Record with ID {pmrId} does not exist.");
            }
        }

        private async Task ValidateRegimenLevel(byte? regimenLevel)
        {
            if (!regimenLevel.HasValue || regimenLevel.Value < 1 || regimenLevel.Value > 3)
            {
                throw new ArgumentException("RegimenLevel must be between 1 and 3");
            }
            await Task.CompletedTask;
        }

        private async Task ValidateRegimenStatus(byte? regimenStatus)
        {
            if (!regimenStatus.HasValue || regimenStatus.Value < 0 || regimenStatus.Value > 2)
            {
                throw new ArgumentException("RegimenStatus must be between 0 and 2");
            }
            await Task.CompletedTask;
        }

        private PatientArvRegimen MapToEntity(PatientArvRegimenRequestDTO requestDTO)
        {
            return new PatientArvRegimen
            {
                PmrId = requestDTO.PatientMedRecordId,
                Notes = requestDTO.Notes,
                RegimenLevel = requestDTO.RegimenLevel,
                CreatedAt = requestDTO.CreatedAt ?? DateTime.UtcNow,
                StartDate = requestDTO.StartDate,
                EndDate = requestDTO.EndDate,
                RegimenStatus = requestDTO.RegimenStatus,
                TotalCost = requestDTO.TotalCost
            };
        }

        private PatientArvRegimenResponseDTO MapToResponseDTO(PatientArvRegimen entity)
        {
            return new PatientArvRegimenResponseDTO
            {
                PatientArvRegiId = entity.ParId,
                PatientMedRecordId = entity.PmrId,
                Notes = entity.Notes,
                RegimenLevel = entity.RegimenLevel,
                CreatedAt = entity.CreatedAt,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                RegimenStatus = entity.RegimenStatus,
                TotalCost = entity.TotalCost
            };
        }

        public async Task<PatientArvRegimenResponseDTO> CreatePatientArvRegimenAsync(PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen));

            // Validate PmrId
            if (patientArvRegimen.PatientMedRecordId <= 0)
                throw new ArgumentException("Invalid Patient Medical Record ID");

            try
            {
                // Validate that the PatientMedicalRecord exists before creating the regimen
                await ValidatePatientMedicalRecordExists(patientArvRegimen.PatientMedRecordId);

                // Validate RegimenLevel
                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);
                var entity = MapToEntity(patientArvRegimen);
                var createdEntity = await _patientArvRegimenRepo.CreatePatientArvRegimenAsync(entity);
                return MapToResponseDTO(createdEntity);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while creating ARV regimen: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error creating ARV regimen: {ex.Message}");
            }
        }

        public async Task<bool> DeletePatientArvRegimenAsync(int parId)
        {
            return await _patientArvRegimenRepo.DeletePatientArvRegimenAsync(parId);
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetAllPatientArvRegimensAsync()
        {
            var entities = await _patientArvRegimenRepo.GetAllPatientArvRegimensAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<PatientArvRegimenResponseDTO?> GetPatientArvRegimenByIdAsync(int parId)
        {
            var entity = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
            if (entity == null)
                return null;
            return MapToResponseDTO(entity);
        }

        public async Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen));

            // Validate PmrId
            if (patientArvRegimen.PatientMedRecordId <= 0)
                throw new ArgumentException("Invalid Patient Medical Record ID");

            try
            {
                // Validate that the PatientMedicalRecord exists before updating the regimen
                await ValidatePatientMedicalRecordExists(patientArvRegimen.PatientMedRecordId);

                // Validate RegimenLevel
                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);
                var entity = MapToEntity(patientArvRegimen);
                var updatedEntity = await _patientArvRegimenRepo.UpdatePatientArvRegimenAsync(parId, entity);
                return MapToResponseDTO(updatedEntity);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while updating ARV regimen: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating ARV regimen: {ex.Message}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetPatientArvRegimensByPatientIdAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    throw new ArgumentException("Invalid Patient ID");

                var entityList = await _patientArvRegimenRepo.GetPatientArvRegimensByPatientIdAsync(patientId);
                if (entityList == null || !entityList.Any())
                    return null;
                return entityList.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving ARV regimens for patient {patientId}: {ex.Message}");
            }
        }
    }
}
