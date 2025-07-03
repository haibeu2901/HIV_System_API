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

        private void ValidateArvMedicationDetailDTO(ArvMedicationDetailDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "ARV Medication Detail cannot be null.");

            if (string.IsNullOrWhiteSpace(dto.ARVMedicationName))
                throw new ArgumentException("ARV Medication Name is required.", nameof(dto.ARVMedicationName));

            if (dto.ARVMedicationPrice < 0)
                throw new ArgumentException("ARV Medication Price cannot be negative.", nameof(dto.ARVMedicationPrice));
        }

        private ArvMedicationDetail MapToEntity(ArvMedicationDetailDTO dto)
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

        private ArvMedicationDetailDTO MapToResponseDTO(ArvMedicationDetail entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "ARV Medication Detail entity cannot be null.");

            return new ArvMedicationDetailDTO
            {
                ARVMedicationName = entity.MedName,
                ARVMedicationDescription = entity.MedDescription,
                ARVMedicationDosage = entity.Dosage,
                ARVMedicationPrice = entity.Price,
                ARVMedicationManufacturer = entity.Manufactorer
            };
        }

        public async Task<ArvMedicationDetailDTO> CreateArvMedicationDetailAsync(ArvMedicationDetailDTO arvMedicationDetail)
        {
            ValidateArvMedicationDetailDTO(arvMedicationDetail);
            var entity = MapToEntity(arvMedicationDetail);
            var created = await _arvMedicationDetailRepo.CreateArvMedicationDetailAsync(entity);
            return MapToResponseDTO(created);
        }

        public async Task<bool> DeleteArvMedicationDetailAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ARV Medication Detail ID.", nameof(id));
            return await _arvMedicationDetailRepo.DeleteArvMedicationDetailAsync(id);
        }

        public async Task<List<ArvMedicationDetailDTO>> GetAllArvMedicationDetailsAsync()
        {
            var entities = await _arvMedicationDetailRepo.GetAllArvMedicationDetailsAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<ArvMedicationDetailDTO> GetArvMedicationDetailByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ARV Medication Detail ID.", nameof(id));
            var entity = await _arvMedicationDetailRepo.GetArvMedicationDetailByIdAsync(id);
            return entity != null ? MapToResponseDTO(entity) : null;
        }

        public async Task<List<ArvMedicationDetailDTO>> SearchArvMedicationDetailsByNameAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty.", nameof(searchTerm));
            var entities = await _arvMedicationDetailRepo.SearchArvMedicationDetailsByNameAsync(searchTerm);
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<ArvMedicationDetailDTO> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetailDTO arvMedicationDetail)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid ARV Medication Detail ID.", nameof(id));
            ValidateArvMedicationDetailDTO(arvMedicationDetail);
            var entity = MapToEntity(arvMedicationDetail);
            var updated = await _arvMedicationDetailRepo.UpdateArvMedicationDetailAsync(id, entity);
            return updated != null ? MapToResponseDTO(updated) : null;
        }
    }
}
