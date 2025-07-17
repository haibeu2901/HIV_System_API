using HIV_System_API_BOs;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using HIV_System_API_DTOs.PatientArvMedicationDTO;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class PatientArvRegimenService : IPatientArvRegimenService
    {
        private readonly IPatientArvRegimenRepo _patientArvRegimenRepo;
        private readonly IPatientArvMedicationRepo _patientArvMedicationRepo;
        private readonly INotificationRepo _notificationRepo;
        private readonly HivSystemApiContext _context; // Add this field

        public PatientArvRegimenService()
        {
            _patientArvRegimenRepo = new PatientArvRegimenRepo() ?? throw new ArgumentNullException(nameof(_patientArvRegimenRepo));
            _patientArvMedicationRepo = new PatientArvMedicationRepo() ?? throw new ArgumentNullException(nameof(_patientArvMedicationRepo));
            _notificationRepo = new NotificationRepo() ?? throw new ArgumentNullException(nameof(_notificationRepo));
            _context = new HivSystemApiContext() ?? throw new ArgumentNullException(nameof(_context)); // Initialize context
        }

        // Use for unit testing or dependency injection
        public PatientArvRegimenService(
            IPatientArvRegimenRepo patientArvRegimenRepo,
            IPatientArvMedicationRepo patientArvMedicationRepo,
            INotificationRepo notificationRepo,
            HivSystemApiContext context)
        {
            _patientArvRegimenRepo = patientArvRegimenRepo ?? throw new ArgumentNullException(nameof(patientArvRegimenRepo));
            _patientArvMedicationRepo = patientArvMedicationRepo ?? throw new ArgumentNullException(nameof(patientArvMedicationRepo));
            _notificationRepo = notificationRepo ?? throw new ArgumentNullException(nameof(notificationRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task ValidatePatientMedicalRecordExists(int pmrId)
        {
            var exists = await _context.PatientMedicalRecords.AnyAsync(pmr => pmr.PmrId == pmrId);
            if (!exists)
            {
                throw new InvalidOperationException($"Hồ sơ y tế bệnh nhân với ID {pmrId} không tồn tại.");
            }
        }

        private async Task ValidatePatientExists(int patientId)
        {
            var exists = await _context.Patients.AnyAsync(p => p.PtnId == patientId);
            if (!exists)
            {
                throw new InvalidOperationException($"Bệnh nhân với ID {patientId} không tồn tại.");
            }
        }

        private async Task ValidateRegimenLevel(byte? regimenLevel)
        {
            // 1=First-line, 2=Second-line, 3=Third-line, 4=SpecialCase
            if (regimenLevel.HasValue && (regimenLevel.Value < 1 || regimenLevel.Value > 4))
            {
                throw new ArgumentException("Cấp độ phác đồ phải từ 1 đến 4");
            }
            await Task.CompletedTask;
        }

        private async Task ValidateRegimenStatus(byte? regimenStatus)
        {
            // 1=Planned, 2=Active, 3=Paused, 4=Failed, 5=Completed
            if (regimenStatus.HasValue && (regimenStatus.Value < 1 || regimenStatus.Value > 5))
            {
                throw new ArgumentException("Trạng thái phác đồ phải từ 1 đến 5");
            }
            await Task.CompletedTask;
        }

        private async Task ValidateArvMedicationDetailExists(int amdId)
        {
            if (amdId <= 0)
                throw new ArgumentException("ID chi tiết thuốc ARV phải lớn hơn 0.", nameof(amdId));
            var exists = await _context.ArvMedicationDetails.AnyAsync(amd => amd.AmdId == amdId);
            if (!exists)
                throw new InvalidOperationException($"Chi tiết thuốc ARV với ID {amdId} không tồn tại.");
        }

        private async Task<double> CalculateTotalCostAsync(int parId)
        {
            var medications = await _context.PatientArvMedications
                .Where(pam => pam.ParId == parId)
                .Include(pam => pam.Amd)
                .ToListAsync();
            return medications.Sum(m => m.Quantity * (double?)m.Amd?.Price ?? 0);
        }

        private PatientArvRegimen MapToEntity(PatientArvRegimenRequestDTO requestDTO)
        {
            return new PatientArvRegimen
            {
                PmrId = requestDTO.PatientMedRecordId,
                Notes = requestDTO.Notes,
                RegimenLevel = requestDTO.RegimenLevel,
                CreatedAt = requestDTO.CreatedAt ?? DateTime.UtcNow,
                StartDate = requestDTO.StartDate,
                EndDate = requestDTO.EndDate,
                RegimenStatus = requestDTO.RegimenStatus,
                TotalCost = requestDTO.TotalCost
            };
        }

        private PatientArvRegimenResponseDTO MapToResponseDTO(PatientArvRegimen entity)
        {
            // Get all ARV medications for this regimen
            var arvMedications = _context.PatientArvMedications
                .Where(pam => pam.ParId == entity.ParId)
                .Select(pam => new PatientArvMedicationResponseDTO
                {
                    PatientArvMedId = pam.PamId,
                    PatientArvRegiId = pam.ParId,
                    ArvMedId = pam.AmdId,
                    Quantity = pam.Quantity,
                    MedicationDetail = pam.Amd != null ? new ArvMedicationDetailResponseDTO
                    {
                        ARVMedicationName = pam.Amd.MedName,
                        ARVMedicationDescription = pam.Amd.MedDescription,
                        ARVMedicationDosage = pam.Amd.Dosage,
                        ARVMedicationPrice = pam.Amd.Price,
                        ARVMedicationManufacturer = pam.Amd.Manufactorer
                    } : null
                })
                .ToList();

            return new PatientArvRegimenResponseDTO
            {
                PatientArvRegiId = entity.ParId,
                PatientMedRecordId = entity.PmrId,
                Notes = entity.Notes,
                RegimenLevel = entity.RegimenLevel,
                CreatedAt = entity.CreatedAt,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                RegimenStatus = entity.RegimenStatus,
                TotalCost = entity.TotalCost,
                ARVMedications = arvMedications
            };
        }

        public async Task<PatientArvRegimenResponseDTO> CreatePatientArvRegimenAsync(PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen));

            // Validate PmrId
            if (patientArvRegimen.PatientMedRecordId <= 0)
                throw new ArgumentException("ID hồ sơ y tế bệnh nhân không hợp lệ");

            try
            {
                // Validate that the PatientMedicalRecord exists before creating the regimen
                await ValidatePatientMedicalRecordExists(patientArvRegimen.PatientMedRecordId);

                // Validate RegimenLevel and RegimenStatus
                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);

                // Validate date logic
                if (patientArvRegimen.StartDate.HasValue && patientArvRegimen.EndDate.HasValue
                    && patientArvRegimen.StartDate > patientArvRegimen.EndDate)
                {
                    throw new ArgumentException("Ngày bắt đầu không thể muộn hơn ngày kết thúc.");
                }

                var entity = MapToEntity(patientArvRegimen);
                var createdEntity = await _patientArvRegimenRepo.CreatePatientArvRegimenAsync(entity);
                return MapToResponseDTO(createdEntity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi tạo phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi tạo phác đồ ARV: {ex.InnerException}");
            }
        }

        public async Task<bool> DeletePatientArvRegimenAsync(int parId)
        {
            try
            {
                // Validate input parameter
                if (parId <= 0)
                {
                    throw new ArgumentException("ID phác đồ ARV bệnh nhân không hợp lệ. ID phải lớn hơn 0.");
                }

                // Check if the regimen exists before attempting deletion
                var existingRegimen = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
                if (existingRegimen == null)
                {
                    throw new InvalidOperationException($"Phác đồ ARV bệnh nhân với ID {parId} không tồn tại.");
                }

                // Check for dependent records (PatientArvMedications) before deletion
                var hasDependentMedications = await _context.PatientArvMedications
                    .AnyAsync(pam => pam.ParId == parId);

                if (hasDependentMedications)
                {
                    throw new InvalidOperationException($"Không thể xóa phác đồ ARV với ID {parId} vì có các thuốc liên kết. Vui lòng xóa tất cả thuốc trước.");
                }

                // Proceed with deletion
                return await _patientArvRegimenRepo.DeletePatientArvRegimenAsync(parId);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi xóa phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi xóa phác đồ ARV: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetAllPatientArvRegimensAsync()
        {
            try
            {
                var entities = await _patientArvRegimenRepo.GetAllPatientArvRegimensAsync();

                if (entities == null || !entities.Any())
                {
                    return new List<PatientArvRegimenResponseDTO>(); // Return empty list instead of null
                }

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi lấy tất cả phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi lấy tất cả phác đồ ARV: {ex.InnerException}");
            }
        }

        public async Task<PatientArvRegimenResponseDTO?> GetPatientArvRegimenByIdAsync(int parId)
        {
            try
            {
                // Validate input parameter
                if (parId <= 0)
                {
                    throw new ArgumentException("ID phác đồ ARV bệnh nhân không hợp lệ. ID phải lớn hơn 0.");
                }

                var entity = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);

                if (entity == null)
                {
                    return null; // Return null if not found (handled by controller)
                }

                return MapToResponseDTO(entity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi lấy phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi lấy phác đồ ARV: {ex.InnerException}");
            }
        }

        public async Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenAsync(int parId, PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen));

            // Validate parId
            if (parId <= 0)
                throw new ArgumentException("ID phác đồ ARV bệnh nhân không hợp lệ");

            // Validate PmrId
            if (patientArvRegimen.PatientMedRecordId <= 0)
                throw new ArgumentException("ID hồ sơ y tế bệnh nhân không hợp lệ");

            try
            {
                // Check if the regimen exists before updating
                var existingRegimen = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
                if (existingRegimen == null)
                {
                    throw new InvalidOperationException($"Phác đồ ARV bệnh nhân với ID {parId} không tồn tại.");
                }

                // Validate the whether the regimen is completed
                if (existingRegimen.RegimenStatus == 5) // Completed
                {
                    throw new InvalidOperationException($"Không thể cập nhật phác đồ ARV với ID {parId} vì đã được đánh dấu là hoàn thành.");
                }

                // Validate that the PatientMedicalRecord exists before updating the regimen
                await ValidatePatientMedicalRecordExists(patientArvRegimen.PatientMedRecordId);

                // Validate RegimenLevel and RegimenStatus
                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);

                // Validate date logic
                if (patientArvRegimen.StartDate.HasValue && patientArvRegimen.EndDate.HasValue
                    && patientArvRegimen.StartDate > patientArvRegimen.EndDate)
                {
                    throw new ArgumentException("Ngày bắt đầu không thể muộn hơn ngày kết thúc.");
                }

                var entity = MapToEntity(patientArvRegimen);
                var updatedEntity = await _patientArvRegimenRepo.UpdatePatientArvRegimenAsync(parId, entity);
                return MapToResponseDTO(updatedEntity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi cập nhật phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi cập nhật phác đồ ARV: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetPatientArvRegimensByPatientIdAsync(int patientId)
        {
            try
            {
                if (patientId <= 0)
                    throw new ArgumentException("ID bệnh nhân không hợp lệ");

                // Validate that the patient exists
                await ValidatePatientExists(patientId);

                var entityList = await _patientArvRegimenRepo.GetPatientArvRegimensByPatientIdAsync(patientId);

                if (entityList == null || !entityList.Any())
                {
                    return new List<PatientArvRegimenResponseDTO>(); // Return empty list instead of null
                }

                return entityList.Select(MapToResponseDTO).ToList();
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi lấy phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi khi lấy phác đồ ARV cho bệnh nhân {patientId}: {ex.InnerException}");
            }
        }

        public async Task<List<PatientArvRegimenResponseDTO>> GetPersonalArvRegimensAsync(int personalId)
        {
            try
            {
                // Validate input parameter
                if (personalId <= 0)
                {
                    throw new ArgumentException("ID cá nhân không hợp lệ. ID phải lớn hơn 0.");
                }

                // Validate that the patient exists
                await ValidatePatientExists(personalId);

                var entityList = await _patientArvRegimenRepo.GetPersonalArvRegimensAsync(personalId);

                if (entityList == null || !entityList.Any())
                {
                    return new List<PatientArvRegimenResponseDTO>(); // Return empty list instead of null
                }

                return entityList.Select(MapToResponseDTO).ToList();
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi lấy phác đồ ARV cá nhân: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi lấy phác đồ ARV cá nhân cho ID cá nhân {personalId}: {ex.InnerException}");
            }
        }

        public async Task<PatientArvRegimenResponseDTO> PatchPatientArvRegimenAsync(int parId, PatientArvRegimenPatchDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
                throw new ArgumentNullException(nameof(patientArvRegimen));

            if (parId <= 0)
                throw new ArgumentException("ID phác đồ ARV bệnh nhân không hợp lệ");

            try
            {
                var existingRegimen = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
                if (existingRegimen == null)
                {
                    throw new InvalidOperationException($"Phác đồ ARV bệnh nhân với ID {parId} không tồn tại.");
                }

                if (existingRegimen.RegimenStatus == 5) // Completed
                {
                    throw new InvalidOperationException($"Không thể cập nhật phác đồ ARV với ID {parId} vì đã được đánh dấu là hoàn thành.");
                }

                if (patientArvRegimen.StartDate.HasValue && existingRegimen.RegimenStatus == 2) // Active
                {
                    throw new InvalidOperationException($"Không thể cập nhật ngày bắt đầu cho phác đồ ARV với ID {parId} vì đang hoạt động.");
                }

                await ValidateRegimenLevel(patientArvRegimen.RegimenLevel);
                await ValidateRegimenStatus(patientArvRegimen.RegimenStatus);

                if (patientArvRegimen.StartDate.HasValue && patientArvRegimen.EndDate.HasValue
                    && patientArvRegimen.StartDate > patientArvRegimen.EndDate)
                {
                    throw new ArgumentException("Ngày bắt đầu không thể muộn hơn ngày kết thúc.");
                }

                // Update only provided fields
                if (patientArvRegimen.Notes != null)
                    existingRegimen.Notes = patientArvRegimen.Notes;
                if (patientArvRegimen.RegimenLevel.HasValue)
                    existingRegimen.RegimenLevel = patientArvRegimen.RegimenLevel.Value;
                if (patientArvRegimen.StartDate.HasValue)
                    existingRegimen.StartDate = patientArvRegimen.StartDate.Value;
                if (patientArvRegimen.EndDate.HasValue)
                    existingRegimen.EndDate = patientArvRegimen.EndDate.Value;
                if (patientArvRegimen.RegimenStatus.HasValue)
                    existingRegimen.RegimenStatus = patientArvRegimen.RegimenStatus.Value;
                if (patientArvRegimen.TotalCost.HasValue)
                    existingRegimen.TotalCost = patientArvRegimen.TotalCost.Value;

                var updatedEntity = await _patientArvRegimenRepo.UpdatePatientArvRegimenAsync(parId, existingRegimen);
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
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi cập nhật phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi cập nhật phác đồ ARV: {ex.InnerException}");
            }
        }

        public async Task<PatientArvRegimenResponseDTO> InitiatePatientArvRegimenAsync(int patientId)
        {
            try
            {
                // Validate input parameter
                if (patientId <= 0)
                {
                    throw new ArgumentException("ID bệnh nhân không hợp lệ. ID phải lớn hơn 0.");
                }

                // Validate that the patient exists
                await ValidatePatientExists(patientId);

                //// Check if patient already has an active ARV regimen
                var existingActiveRegimens = await _patientArvRegimenRepo.GetPatientArvRegimensByPatientIdAsync(patientId);
                var hasActiveRegimen = existingActiveRegimens?.Any(r => r.RegimenStatus == 2) == true; // Status 2 = Active

                if (hasActiveRegimen)
                {
                    throw new InvalidOperationException($"Bệnh nhân với ID {patientId} đã có phác đồ ARV đang hoạt động. Không thể khởi tạo phác đồ mới.");
                }

                // Create the empty ARV regimen through the repository
                var createdEntity = await _patientArvRegimenRepo.InitiatePatientArvRegimenAsync(patientId);

                return MapToResponseDTO(createdEntity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi khởi tạo phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi khởi tạo phác đồ ARV cho bệnh nhân {patientId}: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new ARV regimen with associated medications in a single transaction.
        /// </summary>
        /// <param name="regimenRequest">The ARV regimen data.</param>
        /// <param name="medicationRequests">List of ARV medications to associate with the regimen.</param>
        /// <param name="accId">The account ID of the user performing the action (e.g., doctor or staff).</param>
        /// <returns>The created ARV regimen with associated medications as a response DTO.</returns>
        /// <exception cref="ArgumentNullException">Thrown when regimenRequest or medicationRequests is null.</exception>
        /// <exception cref="ArgumentException">Thrown when input data is invalid.</exception>
        /// <exception cref="InvalidOperationException">Thrown when validations fail or database errors occur.</exception>
        public async Task<PatientArvRegimenResponseDTO> CreatePatientArvRegimenWithMedicationsAsync(
            PatientArvRegimenRequestDTO regimenRequest,
            List<PatientArvMedicationRequestDTO> medicationRequests,
            int accId)
        {
            Debug.WriteLine($"Creating ARV regimen with medications for AccountId: {accId}");

            if (regimenRequest == null)
                throw new ArgumentNullException(nameof(regimenRequest), "Yêu cầu phác đồ là bắt buộc.");
            if (medicationRequests == null || !medicationRequests.Any())
                throw new ArgumentNullException(nameof(medicationRequests), "Ít nhất một yêu cầu thuốc là bắt buộc.");

            // Validate regimen inputs
            if (regimenRequest.PatientMedRecordId <= 0)
                throw new ArgumentException("ID hồ sơ y tế bệnh nhân không hợp lệ", nameof(regimenRequest.PatientMedRecordId));
            await ValidatePatientMedicalRecordExists(regimenRequest.PatientMedRecordId);
            await ValidateRegimenLevel(regimenRequest.RegimenLevel);
            await ValidateRegimenStatus(regimenRequest.RegimenStatus);
            if (regimenRequest.StartDate.HasValue && regimenRequest.EndDate.HasValue
                && regimenRequest.StartDate > regimenRequest.EndDate)
                throw new ArgumentException("Ngày bắt đầu không thể muộn hơn ngày kết thúc.");

            // Validate medication inputs
            foreach (var med in medicationRequests)
            {
                if (med.ArvMedDetailId <= 0)
                    throw new ArgumentException("ID chi tiết thuốc ARV không hợp lệ", nameof(med.ArvMedDetailId));
                if (!med.Quantity.HasValue || med.Quantity <= 0)
                    throw new ArgumentException("Số lượng phải lớn hơn 0.", nameof(med.Quantity));
                await ValidateArvMedicationDetailExists(med.ArvMedDetailId);
            }

            // Check for duplicate medications
            var medIds = medicationRequests.Select(m => m.ArvMedDetailId).ToList();
            if (medIds.Distinct().Count() != medIds.Count)
                throw new ArgumentException("ID thuốc ARV trùng lặp không được phép trong cùng một phác đồ.");

            // Check if patient has an active regimen
            var existingActiveRegimens = await _patientArvRegimenRepo.GetPatientArvRegimensByPatientIdAsync(
                (await _context.PatientMedicalRecords.FindAsync(regimenRequest.PatientMedRecordId))?.PtnId ?? 0);
            if (existingActiveRegimens?.Any(r => r.RegimenStatus == 2) == true)
                throw new InvalidOperationException($"Bệnh nhân đã có phác đồ ARV đang hoạt động.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Create regimen
                var regimenEntity = MapToEntity(regimenRequest);
                var createdRegimen = await _patientArvRegimenRepo.CreatePatientArvRegimenAsync(regimenEntity);

                // Create medications
                var medicationEntities = new List<PatientArvMedication>();
                foreach (var medRequest in medicationRequests)
                {
                    var medEntity = new PatientArvMedication
                    {
                        ParId = createdRegimen.ParId,
                        AmdId = medRequest.ArvMedDetailId,
                        Quantity = medRequest.Quantity
                    };
                    var createdMed = await _patientArvMedicationRepo.CreatePatientArvMedicationAsync(medEntity);
                    medicationEntities.Add(createdMed);
                }

                // Update TotalCost
                var totalCost = await CalculateTotalCostAsync(createdRegimen.ParId);
                createdRegimen.TotalCost = totalCost;
                await _patientArvRegimenRepo.UpdatePatientArvRegimenAsync(createdRegimen.ParId, createdRegimen);

                // Create notification
                var notification = new Notification
                {
                    NotiType = "ARV Regimen Created",
                    NotiMessage = $"A new ARV regimen with {medicationEntities.Count} medication(s) has been created.",
                    SendAt = DateTime.UtcNow
                };
                var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

                // Send notification to patient
                var medicalRecord = await _context.PatientMedicalRecords
                    .Include(pmr => pmr.Ptn)
                    .FirstOrDefaultAsync(pmr => pmr.PmrId == regimenRequest.PatientMedRecordId);
                if (medicalRecord?.Ptn != null)
                    await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, medicalRecord.Ptn.AccId);

                // Send notification to doctor (if creator is not a doctor)
                var account = await _context.Accounts.FindAsync(accId);
                if (account != null && account.Roles != 2) // 2 = Doctor
                {
                    var activeAppointments = await _context.Appointments
                        .Include(a => a.Dct)
                        .Where(a => a.PtnId == medicalRecord.PtnId && (a.ApmStatus == 2 || a.ApmStatus == 3)) // 2, 3 = scheduled/re-scheduled
                        .FirstOrDefaultAsync();
                    if (activeAppointments?.Dct != null)
                        await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, activeAppointments.Dct.AccId);
                }

                await transaction.CommitAsync();
                return MapToResponseDTO(createdRegimen);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to create ARV regimen with medications: {ex.Message}");
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<PatientArvRegimenResponseDTO> UpdatePatientArvRegimenStatusAsync(int parId, PatientArvRegimenStatusRequestDTO request)
        {
            if (parId <= 0)
                throw new ArgumentException("ID phác đồ ARV bệnh nhân không hợp lệ");
            // Validate status
            await ValidateRegimenStatus(request.RegimenStatus);
            try
            {
                var existingRegimen = await _patientArvRegimenRepo.GetPatientArvRegimenByIdAsync(parId);
                if (existingRegimen == null)
                {
                    throw new InvalidOperationException($"Phác đồ ARV bệnh nhân với ID {parId} không tồn tại.");
                }
                // Check if the regimen is already completed
                if (existingRegimen.RegimenStatus == 5) // Completed
                {
                    throw new InvalidOperationException($"Không thể cập nhật phác đồ ARV với ID {parId} vì đã được đánh dấu là hoàn thành.");
                }
                // Update status and notes
                existingRegimen.RegimenStatus = request.RegimenStatus;
                existingRegimen.Notes = request.Notes;
                var updatedEntity = await _patientArvRegimenRepo.UpdatePatientArvRegimenAsync(parId, existingRegimen);
                return MapToResponseDTO(updatedEntity);
            }
            catch (ArgumentException)
            {
                throw; // Re-throw validation exceptions
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw business logic exceptions
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi cập nhật trạng thái phác đồ ARV: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Lỗi không mong muốn khi cập nhật trạng thái phác đồ ARV: {ex.InnerException}");
            }
        }
    }
}
