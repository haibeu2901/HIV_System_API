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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Yêu cầu DTO mẫu thuốc không được để trống.");

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
                throw new ArgumentNullException(nameof(entity), "Thực thể mẫu thuốc không được để trống.");

            // Attempt to use navigation properties if available, otherwise fetch from repositories
            var medicationDetail = entity.Amd ?? await _medicationDetailRepo.GetArvMedicationDetailByIdAsync(entity.AmdId)
                ?? throw new InvalidOperationException($"Không tìm thấy chi tiết thuốc ARV với ID {entity.AmdId}.");
            var regimenTemplate = entity.Art ?? await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(entity.ArtId)
                ?? throw new InvalidOperationException($"Không tìm thấy phác đồ với ID {entity.ArtId}.");

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
                throw new ArgumentNullException(nameof(medicationTemplate), "Yêu cầu DTO mẫu thuốc không được để trống.");

            // Validation
            if (medicationTemplate.ArtId <= 0)
                throw new ArgumentException("ID phác đồ không hợp lệ.", nameof(medicationTemplate.ArtId));

            if (medicationTemplate.AmdId <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ.", nameof(medicationTemplate.AmdId));

            if (medicationTemplate.Quantity.HasValue && medicationTemplate.Quantity < 0)
                throw new ArgumentException("Số lượng không được là số âm.", nameof(medicationTemplate.Quantity));

            // Ensure referenced entities exist
            var regimen = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(medicationTemplate.ArtId);
            if (regimen == null)
                throw new ArgumentException($"Không tìm thấy phác đồ với ID {medicationTemplate.ArtId}.", nameof(medicationTemplate.ArtId));

            var medicationDetail = await _medicationDetailRepo.GetArvMedicationDetailByIdAsync(medicationTemplate.AmdId);
            if (medicationDetail == null)
                throw new ArgumentException($"Không tìm thấy chi tiết thuốc ARV với ID {medicationTemplate.AmdId}.", nameof(medicationTemplate.AmdId));

            // Map DTO to entity
            var entity = MapToEntity(medicationTemplate);

            // Create in repository
            var createdEntity = await _medicationTemplateRepo.CreateMedicationTemplateAsync(entity);
            if (createdEntity == null)
                throw new InvalidOperationException("Không thể tạo mẫu thuốc.");

            // Map to response DTO
            var response = await MapToResponseDTOAsync(createdEntity);

            return response;
        }

        public async Task<bool> DeleteMedicationTemplateAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID mẫu thuốc không hợp lệ.", nameof(id));

            // Check if the entity exists before attempting delete
            var existing = await _medicationTemplateRepo.GetMedicationTemplateByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Không tìm thấy mẫu thuốc với ID {id}.");

            var result = await _medicationTemplateRepo.DeleteMedicationTemplateAsync(id);
            if (!result)
                throw new InvalidOperationException($"Không thể xóa mẫu thuốc với ID {id}.");

            return true;
        }

        public async Task<List<MedicationTemplateResponseDTO>> GetAllMedicationTemplatesAsync()
        {
            try
            {
                var entities = await _medicationTemplateRepo.GetAllMedicationTemplatesAsync();
                if (entities == null || !entities.Any())
                    throw new InvalidOperationException("Không tìm thấy mẫu thuốc nào.");

                var responseList = new List<MedicationTemplateResponseDTO>();
                foreach (var entity in entities)
                {
                    var dto = await MapToResponseDTOAsync(entity);
                    responseList.Add(dto);
                }
                return responseList;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất danh sách mẫu thuốc.", ex);
            }
        }

        public async Task<MedicationTemplateResponseDTO?> GetMedicationTemplateByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID mẫu thuốc không hợp lệ.", nameof(id));

            var entity = await _medicationTemplateRepo.GetMedicationTemplateByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy mẫu thuốc với ID {id}.");

            var dto = await MapToResponseDTOAsync(entity);
            return dto;
        }

        public async Task<List<MedicationTemplateResponseDTO>> GetMedicationTemplatesByAmdIdAsync(int amdId)
        {
            if (amdId <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ.", nameof(amdId));

            var entities = await _medicationTemplateRepo.GetMedicationTemplatesByAmdIdAsync(amdId);
            if (entities == null || !entities.Any())
                throw new InvalidOperationException($"Không tìm thấy mẫu thuốc nào với ID chi tiết thuốc ARV {amdId}.");

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
                throw new ArgumentException("ID phác đồ không hợp lệ.", nameof(artId));

            var entities = await _medicationTemplateRepo.GetMedicationTemplatesByArtIdAsync(artId);
            if (entities == null || !entities.Any())
                throw new InvalidOperationException($"Không tìm thấy mẫu thuốc nào với ID phác đồ {artId}.");

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
                throw new ArgumentNullException(nameof(medicationTemplate), "Yêu cầu DTO mẫu thuốc không được để trống.");

            // Validation
            if (medicationTemplate.ArtId <= 0)
                throw new ArgumentException("ID phác đồ không hợp lệ.", nameof(medicationTemplate.ArtId));

            if (medicationTemplate.AmdId <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ.", nameof(medicationTemplate.AmdId));

            if (medicationTemplate.Quantity.HasValue && medicationTemplate.Quantity < 0)
                throw new ArgumentException("Số lượng không được là số âm.", nameof(medicationTemplate.Quantity));

            // Find existing entity
            var existingTemplates = await _medicationTemplateRepo.GetMedicationTemplatesByArtIdAsync(medicationTemplate.ArtId);
            var entityToUpdate = existingTemplates?.FirstOrDefault(e => e.AmdId == medicationTemplate.AmdId);
            if (entityToUpdate == null)
                throw new KeyNotFoundException($"Không tìm thấy mẫu thuốc với ID phác đồ {medicationTemplate.ArtId} và ID chi tiết thuốc ARV {medicationTemplate.AmdId}.");

            // Ensure referenced entities exist
            var regimen = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(medicationTemplate.ArtId);
            if (regimen == null)
                throw new ArgumentException($"Không tìm thấy phác đồ với ID {medicationTemplate.ArtId}.", nameof(medicationTemplate.ArtId));

            var medicationDetail = await _medicationDetailRepo.GetArvMedicationDetailByIdAsync(medicationTemplate.AmdId);
            if (medicationDetail == null)
                throw new ArgumentException($"Không tìm thấy chi tiết thuốc ARV với ID {medicationTemplate.AmdId}.", nameof(medicationTemplate.AmdId));

            // Update fields
            entityToUpdate.Quantity = medicationTemplate.Quantity;

            // Update in repository
            var updatedEntity = await _medicationTemplateRepo.UpdateMedicationTemplateAsync(entityToUpdate);
            if (updatedEntity == null)
                throw new InvalidOperationException($"Không thể cập nhật mẫu thuốc với ID phác đồ {medicationTemplate.ArtId} và ID chi tiết thuốc ARV {medicationTemplate.AmdId}.");

            // Map to response DTO
            var response = await MapToResponseDTOAsync(updatedEntity);
            return response;
        }
    }
}