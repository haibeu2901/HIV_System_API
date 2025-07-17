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
                throw new ArgumentException("ID phác đồ ARV của bệnh nhân phải lớn hơn 0.", nameof(parId));
            }

            var exists = await _context.PatientArvRegimen.AnyAsync(par => par.ParId == parId);
            if (!exists)
            {
                throw new InvalidOperationException($"Phác đồ ARV của bệnh nhân với ID {parId} không tồn tại.");
            }
        }

        private async Task ValidateArvMedicationDetailExists(int amdId)
        {
            if (amdId <= 0)
            {
                throw new ArgumentException("ID chi tiết thuốc ARV phải lớn hơn 0.", nameof(amdId));
            }

            var exists = await _context.ArvMedicationDetails.AnyAsync(amd => amd.AmdId == amdId);
            if (!exists)
            {
                throw new InvalidOperationException($"Chi tiết thuốc ARV với ID {amdId} không tồn tại.");
            }
        }

        private static void ValidateQuantity(int? quantity)
        {
            if (!quantity.HasValue || quantity.Value <= 0)
            {
                throw new ArgumentException("Số lượng phải lớn hơn 0.");
            }
        }

        private static void ValidateId(int id, string paramName)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"{paramName} phải lớn hơn 0.", paramName);
            }
        }

        private static void ValidateRequestDTO(PatientArvMedicationRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                throw new ArgumentNullException(nameof(requestDTO));
            }

            if (requestDTO.PatientArvRegId <= 0)
            {
                throw new ArgumentException("ID phác đồ ARV của bệnh nhân phải lớn hơn 0.", nameof(requestDTO.PatientArvRegId));
            }

            if (requestDTO.ArvMedDetailId <= 0)
            {
                throw new ArgumentException("ID chi tiết thuốc ARV phải lớn hơn 0.", nameof(requestDTO.ArvMedDetailId));
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
                throw new InvalidOperationException($"Thuốc ARV của bệnh nhân đã tồn tại cho ID phác đồ {parId} và ID thuốc {amdId}.");
            }
        }

        private PatientArvMedication MapToEntity(PatientArvMedicationRequestDTO requestDTO)
        {
            return new PatientArvMedication
            {
                // FIXED: Corrected the mapping - PatientArvMedId should map to ParId, not ParId to ParId
                ParId = requestDTO.PatientArvRegId,
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
                await ValidatePatientArvRegimenExists(patientArvMedication.PatientArvRegId);
                await ValidateArvMedicationDetailExists(patientArvMedication.ArvMedDetailId);

                // Check for duplicate entries
                await ValidateDuplicatePatientArvMedication(patientArvMedication.PatientArvRegId, patientArvMedication.ArvMedDetailId);

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
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi tạo thuốc ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi tạo thuốc ARV: {ex.InnerException}");
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
                    throw new InvalidOperationException($"Thuốc ARV của bệnh nhân với ID {pamId} không tồn tại.");
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
                throw new InvalidOperationException($"Lỗi không mong muốn khi xóa thuốc ARV: {ex.InnerException}");
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
                throw new InvalidOperationException($"Lỗi khi lấy tất cả thuốc ARV: {ex.InnerException}");
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
                throw new InvalidOperationException($"Lỗi khi lấy thuốc ARV: {ex.InnerException}");
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
                    throw new InvalidOperationException($"Thuốc ARV của bệnh nhân với ID {pamId} không tồn tại.");
                }

                // Validate foreign keys exist
                await ValidatePatientArvRegimenExists(patientArvMedication.PatientArvRegId);
                await ValidateArvMedicationDetailExists(patientArvMedication.ArvMedDetailId);

                // Check for duplicate entries (excluding current record)
                await ValidateDuplicatePatientArvMedication(patientArvMedication.PatientArvRegId, patientArvMedication.ArvMedDetailId, pamId);

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
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi cập nhật thuốc ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi cập nhật thuốc ARV: {ex.InnerException}");
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
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi lấy thuốc ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi lấy thuốc ARV: {ex.InnerException}");
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
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi lấy thuốc ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi lấy thuốc ARV: {ex.InnerException}");
            }
        }
    }
}