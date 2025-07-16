using HIV_System_API_BOs;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
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
    public class ArvMedicationDetailService : IArvMedicationDetailService
    {
        private readonly IArvMedicationDetailRepo _arvMedicationDetailRepo;

        public ArvMedicationDetailService()
        {
            _arvMedicationDetailRepo = new ArvMedicationDetailRepo();
        }

        // Use for unit testing or dependency injection
        public ArvMedicationDetailService(IArvMedicationDetailRepo arvMedicationDetailRepo)
        {
            _arvMedicationDetailRepo = arvMedicationDetailRepo; // Injected
        }

        private void ValidateArvMedicationDetailDTO(ArvMedicationDetailResponseDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Chi tiết thuốc ARV không được để trống.");

            if (string.IsNullOrWhiteSpace(dto.ARVMedicationName))
                throw new ArgumentException("Tên thuốc ARV là bắt buộc.", nameof(dto.ARVMedicationName));

            if (dto.ARVMedicationPrice < 0)
                throw new ArgumentException("Giá thuốc ARV không được âm.", nameof(dto.ARVMedicationPrice));
        }

        private ArvMedicationDetail MapToEntity(ArvMedicationDetailResponseDTO dto)
        {
            ValidateArvMedicationDetailDTO(dto);
            return new ArvMedicationDetail
            {
                MedName = dto.ARVMedicationName,
                MedDescription = dto.ARVMedicationDescription,
                Dosage = dto.ARVMedicationDosage,
                Price = dto.ARVMedicationPrice,
                Manufactorer = dto.ARVMedicationManufacturer
            };
        }

        private ArvMedicationDetailResponseDTO MapToResponseDTO(ArvMedicationDetail entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Thực thể chi tiết thuốc ARV không được để trống.");

            return new ArvMedicationDetailResponseDTO
            {
                ARVMedicationName = entity.MedName,
                ARVMedicationDescription = entity.MedDescription,
                ARVMedicationDosage = entity.Dosage,
                ARVMedicationPrice = entity.Price,
                ARVMedicationManufacturer = entity.Manufactorer
            };
        }

        public async Task<ArvMedicationDetailResponseDTO> CreateArvMedicationDetailAsync(ArvMedicationDetailResponseDTO arvMedicationDetail)
        {
            ValidateArvMedicationDetailDTO(arvMedicationDetail);
            var entity = MapToEntity(arvMedicationDetail);
            var created = await _arvMedicationDetailRepo.CreateArvMedicationDetailAsync(entity);
            return MapToResponseDTO(created);
        }

        public async Task<bool> DeleteArvMedicationDetailAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ.", nameof(id));
            return await _arvMedicationDetailRepo.DeleteArvMedicationDetailAsync(id);
        }

        public async Task<List<ArvMedicationDetailResponseDTO>> GetAllArvMedicationDetailsAsync()
        {
            var entities = await _arvMedicationDetailRepo.GetAllArvMedicationDetailsAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<ArvMedicationDetailResponseDTO> GetArvMedicationDetailByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ.", nameof(id));
            var entity = await _arvMedicationDetailRepo.GetArvMedicationDetailByIdAsync(id);
            return entity != null ? MapToResponseDTO(entity) : null;
        }

        public async Task<List<ArvMedicationDetailResponseDTO>> SearchArvMedicationDetailsByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Từ khóa tìm kiếm không được để trống.", nameof(searchTerm));
            var entities = await _arvMedicationDetailRepo.SearchArvMedicationDetailsByNameAsync(searchTerm);
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<ArvMedicationDetailResponseDTO> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetailResponseDTO arvMedicationDetail)
        {
            if (id <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ.", nameof(id));
            ValidateArvMedicationDetailDTO(arvMedicationDetail);
            var entity = MapToEntity(arvMedicationDetail);
            var updated = await _arvMedicationDetailRepo.UpdateArvMedicationDetailAsync(id, entity);
            return updated != null ? MapToResponseDTO(updated) : null;
        }
    }
}