using HIV_System_API_BOs;
using HIV_System_API_DTOs.ARVRegimenDTO;
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
    public class RegimenTemplateService : IRegimenTemplateService
    {
        private readonly IRegimenTemplateRepo _regimenTemplateRepo;

        public RegimenTemplateService()
        {
            _regimenTemplateRepo = new RegimenTemplateRepo();
        }

        private ArvRegimenTemplate MapToEntity(RegimenTemplateRequestDTO regimenTemplate)
        {
            return new ArvRegimenTemplate
            {
                Description = regimenTemplate.Description,
                Level = regimenTemplate.Level,
                Duration = regimenTemplate.Duration,
                ArvMedicationTemplates = new List<ArvMedicationTemplate>()
            };
        }

        private RegimenTemplateResponseDTO MapToResponse(ArvRegimenTemplate regimenTemplate)
        {
            return new RegimenTemplateResponseDTO
            {
                ArtId = regimenTemplate.ArtId,
                Description = regimenTemplate.Description,
                Level = regimenTemplate.Level,
                Duration = regimenTemplate.Duration
                // ArvMedicationTemplates property is not present in RegimenTemplateResponseDTO signature.
            };
        }

        public async Task<RegimenTemplateResponseDTO?> CreateRegimenTemplateAsync(RegimenTemplateRequestDTO regimenTemplate)
        {
            // Validation
            if (regimenTemplate == null)
                throw new ArgumentNullException(nameof(regimenTemplate));

            if (string.IsNullOrWhiteSpace(regimenTemplate.Description))
                throw new ArgumentException("Description is required.", nameof(regimenTemplate.Description));

            if (regimenTemplate.Level == null)
                throw new ArgumentException("Level is required.", nameof(regimenTemplate.Level));

            if (regimenTemplate.Duration == null)
                throw new ArgumentException("Duration is required.", nameof(regimenTemplate.Duration));

            // Map DTO to Entity
            var entity = MapToEntity(regimenTemplate);

            // Persist entity
            var createdEntity = await _regimenTemplateRepo.CreateRegimenTemplateAsync(entity);

            // If creation failed
            if (createdEntity == null)
                return null;

            // Map Entity to Response DTO
            return MapToResponse(createdEntity);
        }

        public async Task<bool> DeleteRegimenTemplateAsync(int id)
        {
            // Validation
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            // Check if the regimen template exists
            var existing = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Regimen template with id {id} not found.");

            // Delete the regimen template
            return await _regimenTemplateRepo.DeleteRegimenTemplateAsync(id);
        }

        public async Task<List<RegimenTemplateResponseDTO>> GetAllRegimenTemplatesAsync()
        {
            // Retrieve all regimen templates from the repository
            var entities = await _regimenTemplateRepo.GetAllRegimenTemplatesAsync();

            // Validation: Check if the result is null
            if (entities == null)
                throw new InvalidOperationException("Failed to retrieve regimen templates.");

            // Map entities to response DTOs
            var responseList = entities
                .Where(e => e != null) // Defensive: filter out any nulls
                .Select(MapToResponse)
                .ToList();

            return responseList;
        }

        public async Task<RegimenTemplateResponseDTO?> GetRegimenTemplateByIdAsync(int id)
        {
            // Validation
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            // Retrieve the regimen template from the repository
            var entity = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(id);

            // If not found, return null
            if (entity == null)
                return null;

            // Map entity to response DTO
            return MapToResponse(entity);
        }

        public async Task<List<RegimenTemplateResponseDTO>> GetRegimenTemplatesByDescriptionAsync(string description)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description must not be null or empty.", nameof(description));

            // Retrieve matching regimen templates from the repository
            var entities = await _regimenTemplateRepo.GetRegimenTemplatesByDescriptionAsync(description);

            // Validation: Check if the result is null
            if (entities == null)
                throw new InvalidOperationException("Failed to retrieve regimen templates by description.");

            // Map entities to response DTOs
            var responseList = entities
                .Where(e => e != null) // Defensive: filter out any nulls
                .Select(MapToResponse)
                .ToList();

            return responseList;
        }

        public async Task<List<RegimenTemplateResponseDTO>> GetRegimenTemplatesByLevelAsync(byte level)
        {
            // Validation
            if (level == 0)
                throw new ArgumentException("Level must be greater than zero.", nameof(level));

            // Retrieve matching regimen templates from the repository
            var entities = await _regimenTemplateRepo.GetRegimenTemplatesByLevelAsync(level);

            // Validation: Check if the result is null
            if (entities == null)
                throw new InvalidOperationException("Failed to retrieve regimen templates by level.");

            // Map entities to response DTOs
            var responseList = entities
                .Where(e => e != null) // Defensive: filter out any nulls
                .Select(MapToResponse)
                .ToList();

            return responseList;
        }

        public async Task<RegimenTemplateResponseDTO?> UpdateRegimenTemplateAsync(int id, RegimenTemplateRequestDTO regimenTemplate)
        {
            // Validation
            if (id <= 0)
                throw new ArgumentException("Id must be greater than zero.", nameof(id));

            if (regimenTemplate == null)
                throw new ArgumentNullException(nameof(regimenTemplate));

            if (string.IsNullOrWhiteSpace(regimenTemplate.Description))
                throw new ArgumentException("Description is required.", nameof(regimenTemplate.Description));

            if (regimenTemplate.Level == null)
                throw new ArgumentException("Level is required.", nameof(regimenTemplate.Level));

            if (regimenTemplate.Duration == null)
                throw new ArgumentException("Duration is required.", nameof(regimenTemplate.Duration));

            // Retrieve existing entity
            var existing = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Regimen template with id {id} not found.");

            // Update fields
            existing.Description = regimenTemplate.Description;
            existing.Level = regimenTemplate.Level;
            existing.Duration = regimenTemplate.Duration;

            // Persist changes
            var updatedEntity = await _regimenTemplateRepo.UpdateRegimenTemplateAsync(id, existing);

            if (updatedEntity == null)
                return null;

            // Map to response DTO
            return MapToResponse(updatedEntity);
        }
    }
}
