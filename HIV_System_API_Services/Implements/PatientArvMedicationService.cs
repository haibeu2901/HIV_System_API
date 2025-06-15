using HIV_System_API_BOs;
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

        private async Task ValidatePatientArvRegimenExists(int parId)
        {
            var exists = await _context.PatientArvRegimen.AnyAsync(par => par.ParId == parId);

            if (!exists)
            {
                throw new InvalidOperationException($"Patient ARV Regimen with ID {parId} does not exist.");
            }
        }

        private async Task ValidateArvMedicationDetailExists(int amdId)
        {
            var exists = await _context.ArvMedicationDetails.AnyAsync(amd => amd.AmdId == amdId);

            if (!exists)
            {
                throw new InvalidOperationException($"ARV Medication Detail with ID {amdId} does not exist.");
            }
        }

        private void ValidateQuantity(int? quantity)
        {
            if (!quantity.HasValue || quantity.Value <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0");
            }
        }

        private PatientArvMedication MapToEntity(PatientArvMedicationRequestDTO requestDTO)
        {
            return new PatientArvMedication
            {
                ParId = requestDTO.PatientArvMedId,
                AmdId = requestDTO.ArvMedDetailId,
                Quantity = requestDTO.Quantity
            };
        }

        private PatientArvMedicationResponseDTO MapToResponseDTO(PatientArvMedication entity)
        {
            return new PatientArvMedicationResponseDTO
            {
                PatientArvMedId = entity.PamId,
                PatientArvRegiId = entity.ParId,
                ArvMedId = entity.AmdId,
                Quantity = entity.Quantity
            };
        }

        public async Task<PatientArvMedicationResponseDTO> CreatePatientArvMedicationAsync(PatientArvMedicationRequestDTO patientArvMedication)
        {
            if (patientArvMedication == null)
                throw new ArgumentNullException(nameof(patientArvMedication));

            try
            {
                // Validate foreign keys exist
                await ValidatePatientArvRegimenExists(patientArvMedication.PatientArvMedId);
                await ValidateArvMedicationDetailExists(patientArvMedication.ArvMedDetailId);
                
                // Validate quantity
                ValidateQuantity(patientArvMedication.Quantity);

                var entity = MapToEntity(patientArvMedication);
                var createdEntity = await _patientArvMedicationRepo.CreatePatientArvMedicationAsync(entity);
                return MapToResponseDTO(createdEntity);
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
                throw new InvalidOperationException($"Unexpected error creating ARV medication: {ex.Message}");
            }
        }

        public async Task<bool> DeletePatientArvMedicationAsync(int pamId)
        {
            return await _patientArvMedicationRepo.DeletePatientArvMedicationAsync(pamId);
        }

        public async Task<List<PatientArvMedicationResponseDTO>> GetAllPatientArvMedicationsAsync()
        {
            var entities = await _patientArvMedicationRepo.GetAllPatientArvMedicationsAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<PatientArvMedicationResponseDTO?> GetPatientArvMedicationByIdAsync(int pamId)
        {
            var entity = await _patientArvMedicationRepo.GetPatientArvMedicationByIdAsync(pamId);
            if (entity == null)
                return null;
            return MapToResponseDTO(entity);
        }

        public async Task<PatientArvMedicationResponseDTO> UpdatePatientArvMedicationAsync(int pamId, PatientArvMedicationRequestDTO patientArvMedication)
        {
            if (patientArvMedication == null)
                throw new ArgumentNullException(nameof(patientArvMedication));

            try
            {
                // Validate foreign keys exist
                await ValidatePatientArvRegimenExists(patientArvMedication.PatientArvMedId);
                await ValidateArvMedicationDetailExists(patientArvMedication.ArvMedDetailId);
                
                // Validate quantity
                ValidateQuantity(patientArvMedication.Quantity);

                var entity = MapToEntity(patientArvMedication);
                var updatedEntity = await _patientArvMedicationRepo.UpdatePatientArvMedicationAsync(pamId, entity);
                return MapToResponseDTO(updatedEntity);
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
                throw new InvalidOperationException($"Unexpected error updating ARV medication: {ex.Message}");
            }
        }
    }
}