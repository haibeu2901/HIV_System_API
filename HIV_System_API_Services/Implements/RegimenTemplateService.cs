using HIV_System_API_BOs;
using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
using HIV_System_API_DTOs.ARVRegimenTemplateDTO;
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
        private readonly IArvMedicationDetailRepo _arvMedicationDetailRepo;

        public RegimenTemplateService()
        {
            _regimenTemplateRepo = new RegimenTemplateRepo();
            _arvMedicationDetailRepo = new ArvMedicationDetailRepo();
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
            if (regimenTemplate == null) 
                throw new ArgumentNullException(nameof(regimenTemplate));

            var medications = regimenTemplate.ArvMedicationTemplates ?? new List<ArvMedicationTemplate>();

            // Map each medication template to response DTO
            var medicationTemplates = medications.Select(medicationTemplate =>
            {
                // Use navigation properties if available, otherwise fetch from repository
                var medicationDetail = medicationTemplate.Amd ??
                    _arvMedicationDetailRepo.GetArvMedicationDetailByIdAsync(medicationTemplate.AmdId).GetAwaiter().GetResult();

                return new MedicationTemplateResponseDTO
                {
                    ArvMedicationTemplateId = medicationTemplate.AmtId,
                    ArvRegimenTemplateId = regimenTemplate.ArtId,
                    ArvRegimenTemplateDescription = regimenTemplate.Description,
                    ArvMedicationDetailId = medicationTemplate.AmdId,
                    MedicationName = medicationDetail?.MedName,
                    MedicationDescription = medicationDetail?.MedDescription,
                    Dosage = medicationDetail?.Dosage,
                    Quantity = medicationTemplate.Quantity ?? 0
                };
            }).ToList();

            return new RegimenTemplateResponseDTO
            {
                ArtId = regimenTemplate.ArtId,
                Description = regimenTemplate.Description,
                Level = regimenTemplate.Level,
                Duration = regimenTemplate.Duration,
                Medications = medicationTemplates
            };
        }

        public async Task<RegimenTemplateResponseDTO?> CreateRegimenTemplateAsync(RegimenTemplateRequestDTO regimenTemplate)
        {
            // Validation
            if (regimenTemplate == null)
                throw new ArgumentNullException(nameof(regimenTemplate));

            if (string.IsNullOrWhiteSpace(regimenTemplate.Description))
                throw new ArgumentException("Mô tả là bắt buộc.", nameof(regimenTemplate.Description));

            if (regimenTemplate.Level == null)
                throw new ArgumentException("Cấp độ là bắt buộc.", nameof(regimenTemplate.Level));

            if (regimenTemplate.Duration == null)
                throw new ArgumentException("Thời gian là bắt buộc.", nameof(regimenTemplate.Duration));

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
                throw new ArgumentException("Id phải lớn hơn 0.", nameof(id));

            // Check if the regimen template exists
            var existing = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Mẫu phác đồ với id {id} không tìm thấy.");

            // Delete the regimen template
            return await _regimenTemplateRepo.DeleteRegimenTemplateAsync(id);
        }

        public async Task<List<RegimenTemplateResponseDTO>> GetAllRegimenTemplatesAsync()
        {
            // Retrieve all regimen templates from the repository
            var entities = await _regimenTemplateRepo.GetAllRegimenTemplatesAsync();

            // Validation: Check if the result is null
            if (entities == null)
                throw new InvalidOperationException("Không thể lấy danh sách mẫu phác đồ.");

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
                throw new ArgumentException("Id phải lớn hơn 0.", nameof(id));

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
                throw new ArgumentException("Mô tả không được null hoặc rỗng.", nameof(description));

            // Retrieve matching regimen templates from the repository
            var entities = await _regimenTemplateRepo.GetRegimenTemplatesByDescriptionAsync(description);

            // Validation: Check if the result is null
            if (entities == null)
                throw new InvalidOperationException("Không thể lấy mẫu phác đồ theo mô tả.");

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
                throw new ArgumentException("Cấp độ phải lớn hơn 0.", nameof(level));

            // Retrieve matching regimen templates from the repository
            var entities = await _regimenTemplateRepo.GetRegimenTemplatesByLevelAsync(level);

            // Validation: Check if the result is null
            if (entities == null)
                throw new InvalidOperationException("Không thể lấy mẫu phác đồ theo cấp độ.");

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
                throw new ArgumentException("Id phải lớn hơn 0.", nameof(id));

            if (regimenTemplate == null)
                throw new ArgumentNullException(nameof(regimenTemplate));

            if (string.IsNullOrWhiteSpace(regimenTemplate.Description))
                throw new ArgumentException("Mô tả là bắt buộc.", nameof(regimenTemplate.Description));

            if (regimenTemplate.Level == null)
                throw new ArgumentException("Cấp độ là bắt buộc.", nameof(regimenTemplate.Level));

            if (regimenTemplate.Duration == null)
                throw new ArgumentException("Thời gian là bắt buộc.", nameof(regimenTemplate.Duration));

            // Retrieve existing entity
            var existing = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Mẫu phác đồ với id {id} không tìm thấy.");

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
