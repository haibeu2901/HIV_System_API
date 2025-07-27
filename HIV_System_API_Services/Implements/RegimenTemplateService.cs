using HIV_System_API_BOs;
using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
using HIV_System_API_DTOs.ARVRegimenTemplateDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly IMedicationTemplateRepo _medicationTemplateRepo;
        private readonly HivSystemApiContext _context;

        public RegimenTemplateService()
        {
            _regimenTemplateRepo = new RegimenTemplateRepo();
            _arvMedicationDetailRepo = new ArvMedicationDetailRepo();
            _medicationTemplateRepo = new MedicationTemplateRepo();
            _context = new HivSystemApiContext();
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
                    MedicationType = medicationDetail?.MedicationType,
                    Dosage = medicationDetail?.Dosage,
                    Quantity = medicationTemplate.Quantity ?? 0,
                    MedicationUsage = medicationTemplate.MedicationUsage
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

        public async Task<RegimenTemplateResponseDTO> CreateRegimenTemplateWithMedicationsTemplate(RegimenTemplateRequestDTO regimenTemplate, List<MedicationTemplateRequestDTO> medicationTemplates)
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

            if (medicationTemplates == null || !medicationTemplates.Any())
                throw new ArgumentException("Danh sách thuốc là bắt buộc.", nameof(medicationTemplates));

            // Map DTO to Entity
            var regimenEntity = MapToEntity(regimenTemplate);

            // Persist regimen template entity
            var createdRegimenEntity = await _regimenTemplateRepo.CreateRegimenTemplateAsync(regimenEntity);

            // If creation failed
            if (createdRegimenEntity == null)
                throw new InvalidOperationException("Không thể tạo mẫu phác đồ.");

            // Create medication templates
            foreach (var medicationTemplate in medicationTemplates)
            {
                // Verify medication detail exists
                var medicationDetail = await _arvMedicationDetailRepo.GetArvMedicationDetailByIdAsync(medicationTemplate.AmdId);
                if (medicationDetail == null)
                    throw new KeyNotFoundException($"Thuốc với id {medicationTemplate.AmdId} không tìm thấy.");

                // Create medication template entity
                var medicationEntity = new ArvMedicationTemplate
                {
                    ArtId = createdRegimenEntity.ArtId,
                    AmdId = medicationTemplate.AmdId,
                    Quantity = medicationTemplate.Quantity,
                    MedicationUsage = medicationTemplate.MedicationUsage
                };

                // Persist medication template entity
                var createdMedicationEntity = await _medicationTemplateRepo.CreateMedicationTemplateAsync(medicationEntity);

                if (createdMedicationEntity == null)
                    throw new InvalidOperationException($"Không thể tạo mẫu thuốc cho thuốc với id {medicationTemplate.AmdId}.");

                // Add to regimen template's medications collection
                createdRegimenEntity.ArvMedicationTemplates.Add(createdMedicationEntity);
            }

            // Map Entity to Response DTO
            return MapToResponse(createdRegimenEntity);
        }

        public async Task<RegimenTemplateResponseDTO> UpdateRegimenTemplateWithMedicationsTemplate(
    int id,
    RegimenTemplateWithMedicationsRequestDTO request)
        {
            // Validation
            if (id <= 0)
                throw new ArgumentException("Id phải lớn hơn 0.", nameof(id));
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Yêu cầu mẫu phác đồ là bắt buộc.");
            if (request.RegimenTemplate == null)
                throw new ArgumentNullException(nameof(request.RegimenTemplate), "Yêu cầu mẫu phác đồ là bắt buộc.");
            if (string.IsNullOrWhiteSpace(request.RegimenTemplate.Description))
                throw new ArgumentException("Mô tả là bắt buộc.", nameof(request.RegimenTemplate.Description));
            if (request.RegimenTemplate.Level == null)
                throw new ArgumentException("Cấp độ là bắt buộc.", nameof(request.RegimenTemplate.Level));
            if (request.RegimenTemplate.Duration == null)
                throw new ArgumentException("Thời gian là bắt buộc.", nameof(request.RegimenTemplate.Duration));
            if (request.MedicationTemplates == null || !request.MedicationTemplates.Any())
                throw new ArgumentNullException(nameof(request.MedicationTemplates), "Ít nhất một yêu cầu thuốc là bắt buộc.");

            var medIds = request.MedicationTemplates.Select(m => m.AmdId).ToList();
            if (medIds.Distinct().Count() != medIds.Count)
                throw new ArgumentException("ID thuốc ARV trùng lặp không được phép trong cùng một mẫu phác đồ.");

            foreach (var med in request.MedicationTemplates)
            {
                if (med.AmdId <= 0)
                    throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ", nameof(med.AmdId));
                if (!med.Quantity.HasValue || med.Quantity <= 0)
                    throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(med.Quantity));
                var medDetail = await _arvMedicationDetailRepo.GetArvMedicationDetailByIdAsync(med.AmdId);
                if (medDetail == null)
                    throw new KeyNotFoundException($"Thuốc với id {med.AmdId} không tìm thấy.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Get existing regimen template
                var existingRegimen = await _regimenTemplateRepo.GetRegimenTemplateByIdAsync(id);
                if (existingRegimen == null)
                    throw new KeyNotFoundException($"Mẫu phác đồ với id {id} không tìm thấy.");

                // UPDATE REGIMEN TEMPLATE FIRST
                existingRegimen.Description = request.RegimenTemplate.Description;
                existingRegimen.Level = request.RegimenTemplate.Level;
                existingRegimen.Duration = request.RegimenTemplate.Duration;

                await _regimenTemplateRepo.UpdateRegimenTemplateAsync(id, existingRegimen);
                Console.WriteLine($"Updated regimen template with id: {id}");

                // THEN UPDATE MEDICATIONS
                // Get current medications
                var currentMedications = await _medicationTemplateRepo.GetMedicationTemplatesByArtIdAsync(id);
                var currentMedDict = currentMedications.ToDictionary(m => m.AmdId, m => m);
                var requestMedDict = request.MedicationTemplates.ToDictionary(m => m.AmdId, m => m);

                // Delete orphaned medications
                foreach (var medToDelete in currentMedications.Where(m => !requestMedDict.ContainsKey(m.AmdId)))
                {
                    if (medToDelete.AmtId <= 0)
                        throw new InvalidOperationException($"Invalid AmtId {medToDelete.AmtId} for medication template to delete.");

                    bool deleted = await _medicationTemplateRepo.DeleteMedicationTemplateAsync(medToDelete.AmtId);
                    if (!deleted)
                    {
                        Console.WriteLine($"Failed to delete medication template with AmtId {medToDelete.AmtId}: Record not found.");
                    }
                }

                // Update or create medications
                var medicationEntities = new List<ArvMedicationTemplate>();
                foreach (var medDto in request.MedicationTemplates)
                {
                    Console.WriteLine($"Processing medication with AmdId: {medDto.AmdId}, Quantity: {medDto.Quantity}");
                    if (currentMedDict.TryGetValue(medDto.AmdId, out var existingMed))
                    {
                        if (existingMed.AmtId <= 0)
                            throw new InvalidOperationException($"Invalid AmtId for existing medication with AmdId {medDto.AmdId}.");

                        var updatedMed = new ArvMedicationTemplate
                        {
                            AmtId = existingMed.AmtId,
                            ArtId = id,
                            AmdId = medDto.AmdId,
                            Quantity = medDto.Quantity,
                            MedicationUsage = medDto.MedicationUsage ?? string.Empty
                        };
                        await _medicationTemplateRepo.UpdateMedicationTemplateAsync(updatedMed);
                        Console.WriteLine($"Updated medication AmtId: {updatedMed.AmtId}, Quantity: {updatedMed.Quantity}");
                        medicationEntities.Add(updatedMed);
                    }
                    else
                    {
                        var newMed = new ArvMedicationTemplate
                        {
                            ArtId = id,
                            AmdId = medDto.AmdId,
                            Quantity = medDto.Quantity,
                            MedicationUsage = medDto.MedicationUsage ?? string.Empty
                        };
                        var createdMed = await _medicationTemplateRepo.CreateMedicationTemplateAsync(newMed);
                        Console.WriteLine($"Created medication AmtId: {createdMed.AmtId}, AmdId: {createdMed.AmdId}, Quantity: {createdMed.Quantity}");
                        medicationEntities.Add(createdMed);
                    }
                }

                await transaction.CommitAsync();

                // Get fresh regimen data with updated medications for response
                existingRegimen.ArvMedicationTemplates = await _medicationTemplateRepo.GetMedicationTemplatesByArtIdAsync(id);
                Console.WriteLine($"Retrieved {existingRegimen.ArvMedicationTemplates.Count} medications for ArtId: {id}");

                return MapToResponse(existingRegimen);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
