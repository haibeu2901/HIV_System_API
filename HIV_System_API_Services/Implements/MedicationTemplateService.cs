using HIV_System_API_BOs;
using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
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
    public class MedicationTemplateService : IMedicationTemplateService
    {
        private readonly IMedicationTemplateRepo _medicationTemplateRepo;
        private readonly IRegimenTemplateRepo _regimenTemplateRepo;
        private readonly IArvMedicationDetailRepo _medicationDetailRepo;

        public MedicationTemplateService()
        {
            _medicationTemplateRepo = new MedicationTemplateRepo();
            _medicationDetailRepo = new ArvMedicationDetailRepo();
            _regimenTemplateRepo = new RegimenTemplateRepo();
        }

        private ArvMedicationTemplate MapToEntity(MedicationTemplateRequestDTO dto)
        {
            return new ArvMedicationTemplate
            {
                AmtId = 0, // Assuming 0 for new entities; set appropriately if updating
                ArtId = dto.ArtId,
                AmdId = dto.AmdId,
                Quantity = dto.Quantity
            };
        }

        private async Task<MedicationTemplateResponseDTO> MapToResponseDTOAsync(ArvMedicationTemplate entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // Attempt to use navigation properties if available, otherwise fetch from repositories
            var medicationDetail = entity.Amd ?? await _medicationDetailRepo.GetArvMedicationDetailByIdAsync(entity.AmdId);
            var regimenTemplate = entity.Art ?? await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(entity.ArtId);

            return new MedicationTemplateResponseDTO
            {
                ArvMedicationTemplateId = entity.AmtId,
                ArvRegimenTemplateId = entity.ArtId,
                ArvRegimenTemplateDescription = regimenTemplate?.Description,
                ArvMedicationDetailId = entity.AmdId,
                MedicationName = medicationDetail?.MedName,
                MedicationDescription = medicationDetail?.MedDescription,
                Dosage = medicationDetail?.Dosage,
                Quantity = entity.Quantity ?? 0
            };
        }

        public async Task<MedicationTemplateResponseDTO> CreateMedicationTemplateAsync(MedicationTemplateRequestDTO medicationTemplate)
        {
            if (medicationTemplate == null)
                throw new ArgumentNullException(nameof(medicationTemplate));

            // Validation
            if (medicationTemplate.ArtId <= 0)
                throw new ArgumentException("Id (ArtId) của mẫu phát đồ không hợp lệ.", nameof(medicationTemplate.ArtId));

            if (medicationTemplate.AmdId <= 0)
                throw new ArgumentException("Id (AmdId) mẫu thông tin thuốc không hợp lệ.", nameof(medicationTemplate.AmdId));

            if (medicationTemplate.Quantity is not null && medicationTemplate.Quantity < 0)
                throw new ArgumentException("SỐ lượng không thể âm.", nameof(medicationTemplate.Quantity));

            // Ensure referenced entities exist
            var regimen = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(medicationTemplate.ArtId);
            if (regimen == null)
                throw new ArgumentException($"Mãu phác đồ với Id {medicationTemplate.ArtId} không tồn tại.", nameof(medicationTemplate.ArtId));

            var medicationDetail = await _medicationDetailRepo.GetArvMedicationDetailByIdAsync(medicationTemplate.AmdId);
            if (medicationDetail == null)
                throw new ArgumentException($"Mẫu thông tin thuốc với Id {medicationTemplate.AmdId} không tồn tại.", nameof(medicationTemplate.AmdId));

            // Map DTO to entity
            var entity = MapToEntity(medicationTemplate);

            // Create in repository
            var createdEntity = await _medicationTemplateRepo.CreateMedicationTemplateAsync(entity);

            // Map to response DTO
            var response = await MapToResponseDTOAsync(createdEntity);

            return response;
        }

        public async Task<bool> DeleteMedicationTemplateAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id mẫu thuốc không hợp lệ.", nameof(id));

            // Check if the entity exists before attempting delete
            var existing = await _medicationTemplateRepo.GetMedicationTemplateByIdAsync(id);
            if (existing == null)
                throw new ArgumentException($"Mẫu thuốc với Id {id} không tồn tại.", nameof(id));

            var result = await _medicationTemplateRepo.DeleteMedicationTemplateAsync(id);
            return result;
        }

        public async Task<List<MedicationTemplateResponseDTO>> GetAllMedicationTemplatesAsync()
        {
            var entities = await _medicationTemplateRepo.GetAllMedicationTemplatesAsync();
            if (entities == null || !entities.Any())
                return new List<MedicationTemplateResponseDTO>();

            var responseList = new List<MedicationTemplateResponseDTO>();
            foreach (var entity in entities)
            {
                var dto = await MapToResponseDTOAsync(entity);
                responseList.Add(dto);
            }
            return responseList;
        }

        public async Task<MedicationTemplateResponseDTO?> GetMedicationTemplateByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Id mẫu thuốc không hợp lệ.", nameof(id));

            var entity = await _medicationTemplateRepo.GetMedicationTemplateByIdAsync(id);
            if (entity == null)
                return null;

            var dto = await MapToResponseDTOAsync(entity);
            return dto;
        }

        public async Task<List<MedicationTemplateResponseDTO>> GetMedicationTemplatesByAmdIdAsync(int amdId)
        {
            if (amdId <= 0)
                throw new ArgumentException("Thông tin thuốc Id (AmdId) không hợp lệ.", nameof(amdId));

            var entities = await _medicationTemplateRepo.GetMedicationTemplatesByAmdIdAsync(amdId);
            if (entities == null || !entities.Any())
                return new List<MedicationTemplateResponseDTO>();

            var responseList = new List<MedicationTemplateResponseDTO>();
            foreach (var entity in entities)
            {
                var dto = await MapToResponseDTOAsync(entity);
                responseList.Add(dto);
            }
            return responseList;
        }

        public async Task<List<MedicationTemplateResponseDTO>> GetMedicationTemplatesByArtIdAsync(int artId)
        {
            if (artId <= 0)
                throw new ArgumentException("Mãu phác đồ Id (ArtId) không hợp lệ.", nameof(artId));

            var entities = await _medicationTemplateRepo.GetMedicationTemplatesByArtIdAsync(artId);
            if (entities == null || !entities.Any())
                return new List<MedicationTemplateResponseDTO>();

            var responseList = new List<MedicationTemplateResponseDTO>();
            foreach (var entity in entities)
            {
                var dto = await MapToResponseDTOAsync(entity);
                responseList.Add(dto);
            }
            return responseList;
        }

        public async Task<MedicationTemplateResponseDTO> UpdateMedicationTemplateAsync(MedicationTemplateRequestDTO medicationTemplate)
        {
            if (medicationTemplate == null)
                throw new ArgumentNullException(nameof(medicationTemplate));

            // Validation
            if (medicationTemplate.ArtId <= 0)
                throw new ArgumentException("Mãu phác đồ Id (ArtId) không hợp lệ.", nameof(medicationTemplate.ArtId));

            if (medicationTemplate.AmdId <= 0)
                throw new ArgumentException("Mãu thông tin thuốc không hợp lệ.", nameof(medicationTemplate.AmdId));

            if (medicationTemplate.Quantity is not null && medicationTemplate.Quantity < 0)
                throw new ArgumentException("Số lượng không thể âm.", nameof(medicationTemplate.Quantity));

            // Find existing entity
            var existing = await _medicationTemplateRepo.GetMedicationTemplatesByArtIdAsync(medicationTemplate.ArtId);
            var entityToUpdate = existing?.FirstOrDefault(e => e.AmdId == medicationTemplate.AmdId);
            if (entityToUpdate == null)
                throw new ArgumentException($"Mẫu thuốc với ArtId {medicationTemplate.ArtId} và AmdId {medicationTemplate.AmdId} không tồn tại.");

            // Ensure referenced entities exist
            var regimen = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(medicationTemplate.ArtId);
            if (regimen == null)
                throw new ArgumentException($"Mãu phác đồ với Id {medicationTemplate.ArtId} không tồn tại.", nameof(medicationTemplate.ArtId));

            var medicationDetail = await _medicationDetailRepo.GetArvMedicationDetailByIdAsync(medicationTemplate.AmdId);
            if (medicationDetail == null)
                throw new ArgumentException($"Mẫu thông tin thuốc với Id {medicationTemplate.AmdId} không tồn tại.", nameof(medicationTemplate.AmdId));

            // Update fields
            entityToUpdate.Quantity = medicationTemplate.Quantity;

            // Update in repository
            var updatedEntity = await _medicationTemplateRepo.UpdateMedicationTemplateAsync(entityToUpdate);

            // Map to response DTO
            var response = await MapToResponseDTOAsync(updatedEntity);
            return response;
        }
    }
}
