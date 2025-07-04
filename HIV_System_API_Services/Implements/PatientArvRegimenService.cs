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

        // Use for unit testing or dependency injection
        public PatientArvRegimenService(IPatientArvRegimenRepo patientArvRegimenRepo, HivSystemApiContext context)
        {
            _patientArvRegimenRepo = patientArvRegimenRepo; // Injected
            _context = context;
        }

        private async Task ValidatePatientMedicalRecordExists(int pmrId)
        {
            var exists = await _context.PatientMedicalRecords.AnyAsync(pmr => pmr.PmrId == pmrId);
            if (!exists)
            {
                throw new InvalidOperationException($"Patient Medical Record with ID {pmrId} does not exist.");
            }
        }

        private async Task ValidatePatientExists(int patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.PtnId == patientId);
            if (!exists)
            {
                throw new InvalidOperationException($"Patient with ID {patientId} does not exist.");
            }
        }

        private async Task ValidateRegimenLevel(byte? regimenLevel)
        {
            // 1=First-line, 2=Second-line, 3=Third-line, 4=SpecialCase
            if (regimenLevel.HasValue && (regimenLevel.Value < 1 || regimenLevel.Value > 4))
            {
                throw new ArgumentException("RegimenLevel must be between 1 and 4");
            }
            await Task.CompletedTask;
        }

        private async Task ValidateRegimenStatus(byte? regimenStatus)
        {
            // 1=Planned, 2=Active, 3=Paused, 4=Failed, 5=Completed
            if (regimenStatus.HasValue && (regimenStatus.Value < 1 || regimenStatus.Value > 5))
            {
                throw new ArgumentException("RegimenStatus must be between 1 and 5");
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

                // Validate RegimenLevel and RegimenStatus
                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);

                // Validate date logic
                if (patientArvRegimen.StartDate.HasValue && patientArvRegimen.EndDate.HasValue
                    && patientArvRegimen.StartDate > patientArvRegimen.EndDate)
                {
                    throw new ArgumentException("Start date cannot be later than end date.");
                }

                var entity = MapToEntity(patientArvRegimen);
                var createdEntity = await _patientArvRegimenRepo.CreatePatientArvRegimenAsync(entity);
                return MapToResponseDTO(createdEntity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
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
                throw new InvalidOperationException($"Unexpected error creating ARV regimen: {ex.InnerException}");
            }
        }

        public async Task<bool> DeletePatientArvRegimenAsync(int parId)
        {
            try
            {
                // Validate input parameter
                if (parId <= 0)
                {
                    throw new ArgumentException("Invalid Patient ARV Regimen ID. ID must be greater than 0.");
                }

                // Check if the regimen exists before attempting deletion
                var existingRegimen = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
                if (existingRegimen == null)
                {
                    throw new InvalidOperationException($"Patient ARV Regimen with ID {parId} does not exist.");
                }

                // Check for dependent records (PatientArvMedications) before deletion
                var hasDependentMedications = await _context.PatientArvMedications
                    .AnyAsync(pam => pam.ParId == parId);

                if (hasDependentMedications)
                {
                    throw new InvalidOperationException($"Cannot delete ARV Regimen with ID {parId} because it has associated medications. Please remove all medications first.");
                }

                // Proceed with deletion
                return await _patientArvRegimenRepo.DeletePatientArvRegimenAsync(parId);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while deleting ARV regimen: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deleting ARV regimen: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetAllPatientArvRegimensAsync()
        {
            try
            {
                var entities = await _patientArvRegimenRepo.GetAllPatientArvRegimensAsync();

                if (entities == null || !entities.Any())
                {
                    return new List<PatientArvRegimenResponseDTO>(); // Return empty list instead of null
                }

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while retrieving all ARV regimens: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error retrieving all ARV regimens: {ex.InnerException}");
            }
        }

        public async Task<PatientArvRegimenResponseDTO?> GetPatientArvRegimenByIdAsync(int parId)
        {
            try
            {
                // Validate input parameter
                if (parId <= 0)
                {
                    throw new ArgumentException("Invalid Patient ARV Regimen ID. ID must be greater than 0.");
                }

                var entity = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);

                if (entity == null)
                {
                    return null; // Return null if not found (handled by controller)
                }

                return MapToResponseDTO(entity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while retrieving ARV regimen: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error retrieving ARV regimen: {ex.InnerException}");
            }
        }

        public async Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen));

            // Validate parId
            if (parId <= 0)
                throw new ArgumentException("Invalid Patient ARV Regimen ID");

            // Validate PmrId
            if (patientArvRegimen.PatientMedRecordId <= 0)
                throw new ArgumentException("Invalid Patient Medical Record ID");

            try
            {
                // Check if the regimen exists before updating
                var existingRegimen = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
                if (existingRegimen == null)
                {
                    throw new InvalidOperationException($"Patient ARV Regimen with ID {parId} does not exist.");
                }

                // Validate that the PatientMedicalRecord exists before updating the regimen
                await ValidatePatientMedicalRecordExists(patientArvRegimen.PatientMedRecordId);

                // Validate RegimenLevel and RegimenStatus
                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);

                // Validate date logic
                if (patientArvRegimen.StartDate.HasValue && patientArvRegimen.EndDate.HasValue
                    && patientArvRegimen.StartDate > patientArvRegimen.EndDate)
                {
                    throw new ArgumentException("Start date cannot be later than end date.");
                }

                var entity = MapToEntity(patientArvRegimen);
                var updatedEntity = await _patientArvRegimenRepo.UpdatePatientArvRegimenAsync(parId, entity);
                return MapToResponseDTO(updatedEntity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
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
                throw new InvalidOperationException($"Unexpected error updating ARV regimen: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetPatientArvRegimensByPatientIdAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    throw new ArgumentException("Invalid Patient ID");

                // Validate that the patient exists
                await ValidatePatientExists(patientId);

                var entityList = await _patientArvRegimenRepo.GetPatientArvRegimensByPatientIdAsync(patientId);

                if (entityList == null || !entityList.Any())
                {
                    return new List<PatientArvRegimenResponseDTO>(); // Return empty list instead of null
                }

                return entityList.Select(MapToResponseDTO).ToList();
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while retrieving ARV regimens: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving ARV regimens for patient {patientId}: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetPersonalArvRegimensAsync(int personalId)
        {
            try
            {
                // Validate input parameter
                if (personalId <= 0)
                {
                    throw new ArgumentException("Invalid Personal ID. ID must be greater than 0.");
                }

                // Validate that the patient exists
                await ValidatePatientExists(personalId);

                var entityList = await _patientArvRegimenRepo.GetPersonalArvRegimensAsync(personalId);

                if (entityList == null || !entityList.Any())
                {
                    return new List<PatientArvRegimenResponseDTO>(); // Return empty list instead of null
                }

                return entityList.Select(MapToResponseDTO).ToList();
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while retrieving personal ARV regimens: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error retrieving personal ARV regimens for personal ID {personalId}: {ex.InnerException}");
            }
        }
    }
}
