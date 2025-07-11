using HIV_System_API_BOs;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using HIV_System_API_DTOs.PatientArvMedicationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_Services.Implements
{
    public class PatientArvMedicationService : IPatientArvMedicationService
    {
        private readonly IPatientArvMedicationRepo _patientArvMedicationRepo;
        private readonly HivSystemApiContext _context;

        public PatientArvMedicationService()
        {
            _patientArvMedicationRepo = new PatientArvMedicationRepo();
            _context = new HivSystemApiContext();
        }

        // Constructor for dependency injection
        public PatientArvMedicationService(IPatientArvMedicationRepo patientArvMedicationRepo, HivSystemApiContext context)
        {
            _patientArvMedicationRepo = patientArvMedicationRepo ?? throw new ArgumentNullException(nameof(patientArvMedicationRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task ValidatePatientArvRegimenExists(int parId)
        {
            if (parId <= 0)
            {
                throw new ArgumentException("Patient ARV Regimen ID must be greater than 0.", nameof(parId));
            }

            var exists = await _context.PatientArvRegimen.AnyAsync(par => par.ParId == parId);
            if (!exists)
            {
                throw new InvalidOperationException($"Patient ARV Regimen with ID {parId} does not exist.");
            }
        }

        private async Task ValidateArvMedicationDetailExists(int amdId)
        {
            if (amdId <= 0)
            {
                throw new ArgumentException("ARV Medication Detail ID must be greater than 0.", nameof(amdId));
            }

            var exists = await _context.ArvMedicationDetails.AnyAsync(amd => amd.AmdId == amdId);
            if (!exists)
            {
                throw new InvalidOperationException($"ARV Medication Detail with ID {amdId} does not exist.");
            }
        }

        private static void ValidateQuantity(int? quantity)
        {
            if (!quantity.HasValue || quantity.Value <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0.");
            }
        }

        private static void ValidateId(int id, string paramName)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"{paramName} must be greater than 0.", paramName);
            }
        }

        private static void ValidateRequestDTO(PatientArvMedicationRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                throw new ArgumentNullException(nameof(requestDTO));
            }

            if (requestDTO.PatientArvMedId <= 0)
            {
                throw new ArgumentException("Patient ARV Regimen ID must be greater than 0.", nameof(requestDTO.PatientArvMedId));
            }

            if (requestDTO.ArvMedDetailId <= 0)
            {
                throw new ArgumentException("ARV Medication Detail ID must be greater than 0.", nameof(requestDTO.ArvMedDetailId));
            }

            ValidateQuantity(requestDTO.Quantity);
        }

        private async Task ValidateDuplicatePatientArvMedication(int parId, int amdId, int? excludePamId = null)
        {
            var query = _context.PatientArvMedications
                .Where(pam => pam.ParId == parId && pam.AmdId == amdId);

            if (excludePamId.HasValue)
            {
                query = query.Where(pam => pam.PamId != excludePamId.Value);
            }

            var exists = await query.AnyAsync();
            if (exists)
            {
                throw new InvalidOperationException($"Patient ARV Medication already exists for Regimen ID {parId} and Medication ID {amdId}.");
            }
        }

        private PatientArvMedication MapToEntity(PatientArvMedicationRequestDTO requestDTO)
        {
            return new PatientArvMedication
            {
                // FIXED: Corrected the mapping - PatientArvMedId should map to ParId, not ParId to ParId
                ParId = requestDTO.PatientArvMedId,
                AmdId = requestDTO.ArvMedDetailId,
                Quantity = requestDTO.Quantity
            };
        }

        private PatientArvMedicationResponseDTO MapToResponseDTO(PatientArvMedication entity)
        {
            var responseDTO = new PatientArvMedicationResponseDTO
            {
                PatientArvMedId = entity.PamId,
                PatientArvRegiId = entity.ParId,
                ArvMedId = entity.AmdId,
                Quantity = entity.Quantity
            };

            // Map medication details if available
            if (entity.Amd != null)
            {
                responseDTO.MedicationDetail = new ArvMedicationDetailResponseDTO
                {
                    ARVMedicationName = entity.Amd.MedName,
                    ARVMedicationDescription = entity.Amd.MedDescription,
                    ARVMedicationDosage = entity.Amd.Dosage,
                    ARVMedicationPrice = entity.Amd.Price,
                    ARVMedicationManufacturer = entity.Amd.Manufactorer
                };
            }

            return responseDTO;
        }

        public async Task<PatientArvMedicationResponseDTO> CreatePatientArvMedicationAsync(PatientArvMedicationRequestDTO patientArvMedication)
        {
            try
            {
                ValidateRequestDTO(patientArvMedication);

                // Validate foreign keys exist
                await ValidatePatientArvRegimenExists(patientArvMedication.PatientArvMedId);
                await ValidateArvMedicationDetailExists(patientArvMedication.ArvMedDetailId);

                // Check for duplicate entries
                await ValidateDuplicatePatientArvMedication(patientArvMedication.PatientArvMedId, patientArvMedication.ArvMedDetailId);

                var entity = MapToEntity(patientArvMedication);
                var createdEntity = await _patientArvMedicationRepo.CreatePatientArvMedicationAsync(entity);

                return MapToResponseDTO(createdEntity);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while creating ARV medication: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error creating ARV medication: {ex.InnerException}");
            }
        }

        public async Task<bool> DeletePatientArvMedicationAsync(int pamId)
        {
            try
            {
                ValidateId(pamId, nameof(pamId));

                // Check if the record exists
                var existingEntity = await _patientArvMedicationRepo.GetPatientArvMedicationByIdAsync(pamId);
                if (existingEntity == null)
                {
                    throw new InvalidOperationException($"Patient ARV Medication with ID {pamId} does not exist.");
                }

                return await _patientArvMedicationRepo.DeletePatientArvMedicationAsync(pamId);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error deleting ARV medication: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvMedicationResponseDTO>> GetAllPatientArvMedicationsAsync()
        {
            try
            {
                var entities = await _patientArvMedicationRepo.GetAllPatientArvMedicationsAsync();
                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving all ARV medications: {ex.InnerException}");
            }
        }

        public async Task<PatientArvMedicationResponseDTO?> GetPatientArvMedicationByIdAsync(int pamId)
        {
            try
            {
                ValidateId(pamId, nameof(pamId));

                var entity = await _patientArvMedicationRepo.GetPatientArvMedicationByIdAsync(pamId);
                return entity == null ? null : MapToResponseDTO(entity);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving ARV medication: {ex.InnerException}");
            }
        }

        public async Task<PatientArvMedicationResponseDTO> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedicationRequestDTO patientArvMedication)
        {
            try
            {
                ValidateId(pamId, nameof(pamId));
                ValidateRequestDTO(patientArvMedication);

                // Check if the record exists
                var existingEntity = await _patientArvMedicationRepo.GetPatientArvMedicationByIdAsync(pamId);
                if (existingEntity == null)
                {
                    throw new InvalidOperationException($"Patient ARV Medication with ID {pamId} does not exist.");
                }

                // Validate foreign keys exist
                await ValidatePatientArvRegimenExists(patientArvMedication.PatientArvMedId);
                await ValidateArvMedicationDetailExists(patientArvMedication.ArvMedDetailId);

                // Check for duplicate entries (excluding current record)
                await ValidateDuplicatePatientArvMedication(patientArvMedication.PatientArvMedId, patientArvMedication.ArvMedDetailId, pamId);

                var entity = MapToEntity(patientArvMedication);
                var updatedEntity = await _patientArvMedicationRepo.UpdatePatientArvMedicationAsync(pamId, entity);

                return MapToResponseDTO(updatedEntity);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while updating ARV medication: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating ARV medication: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvMedicationResponseDTO>> GetPatientArvMedicationsByPatientIdAsync(int patientId)
        {
            try
            {
                ValidateId(patientId, nameof(patientId));

                var entities = await _patientArvMedicationRepo.GetPatientArvMedicationsByPatientIdAsync(patientId);
                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while updating ARV medication: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating ARV medication: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvMedicationResponseDTO>> GetPatientArvMedicationsByPatientRegimenIdAsync(int parId)
        {
            try
            {
                ValidateId(parId, nameof(parId));

                var entities = await _patientArvMedicationRepo.GetPatientArvMedicationsByPatientRegimenIdAsync(parId);
                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while updating ARV medication: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating ARV medication: {ex.InnerException}");
            }
        }
    }
}