using HIV_System_API_BOs;
using HIV_System_API_DTOs.MedicalServiceDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class MedicalServiceService : IMedicalServiceService
    {
        private readonly IMedicalServiceRepo _medicalServiceRepo;
        private readonly HivSystemApiContext _context;

        public MedicalServiceService()
        {
            _medicalServiceRepo = new MedicalServiceRepo();
            _context = new HivSystemApiContext();
        }

        private async Task ValidateAccountExists(int accId)
        {
            if (accId <= 0)
                throw new ArgumentException("ID tài khoản không hợp lệ.", nameof(accId));

            var exists = await _context.Accounts.AnyAsync(acc => acc.AccId == accId);
            if (!exists)
            {
                throw new InvalidOperationException($"Không tìm thấy tài khoản với ID {accId}.");
            }
        }

        private void ValidateServiceName(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Tên dịch vụ không được để trống hoặc chỉ chứa khoảng trắng.", nameof(serviceName));
            }
        }

        private void ValidatePrice(double price)
        {
            if (price < 0)
            {
                throw new ArgumentException("Giá không được là số âm.", nameof(price));
            }
        }

        private MedicalService MapToEntity(MedicalServiceRequestDTO requestDTO)
        {
            if (requestDTO == null)
                throw new ArgumentNullException(nameof(requestDTO), "Yêu cầu DTO dịch vụ y tế không được để trống.");

            return new MedicalService
            {
                AccId = requestDTO.AccId,
                ServiceName = requestDTO.ServiceName,
                ServiceDescription = requestDTO.ServiceDescription,
                Price = requestDTO.Price,
                IsAvailable = requestDTO.IsAvailable
            };
        }

        private MedicalServiceResponseDTO MapToResponseDTO(MedicalService entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Thực thể dịch vụ y tế không được để trống.");

            return new MedicalServiceResponseDTO
            {
                ServiceId = entity.SrvId,
                AccId = entity.AccId,
                ServiceName = entity.ServiceName,
                ServiceDescription = entity.ServiceDescription,
                Price = entity.Price,
                IsAvailable = entity.IsAvailable
            };
        }

        public async Task<MedicalServiceResponseDTO> CreateMedicalServiceAsync(MedicalServiceRequestDTO medicalService)
        {
            if (medicalService == null)
                throw new ArgumentNullException(nameof(medicalService), "Yêu cầu DTO dịch vụ y tế không được để trống.");

            try
            {
                await ValidateAccountExists(medicalService.AccId);
                ValidateServiceName(medicalService.ServiceName);
                ValidatePrice(medicalService.Price);

                var entity = MapToEntity(medicalService);
                var createdEntity = await _medicalServiceRepo.CreateMedicalServiceAsync(entity);
                if (createdEntity == null)
                    throw new InvalidOperationException("Không thể tạo dịch vụ y tế.");

                return MapToResponseDTO(createdEntity);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi tạo dịch vụ y tế: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Đã xảy ra lỗi khi tạo dịch vụ y tế.", ex);
            }
        }

        public async Task<bool> DeleteMedicalServiceAsync(int srvId)
        {
            if (srvId <= 0)
                throw new ArgumentException("ID dịch vụ y tế không hợp lệ.", nameof(srvId));

            try
            {
                var result = await _medicalServiceRepo.DeleteMedicalServiceAsync(srvId);
                if (!result)
                    throw new KeyNotFoundException($"Không tìm thấy dịch vụ y tế với ID {srvId} để xóa.");

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể xóa dịch vụ y tế với ID {srvId}.", ex);
            }
        }

        public async Task<List<MedicalServiceResponseDTO>> GetAllMedicalServicesAsync()
        {
            try
            {
                var entities = await _medicalServiceRepo.GetAllMedicalServicesAsync();
                if (entities == null || !entities.Any())
                    throw new InvalidOperationException("Không tìm thấy dịch vụ y tế nào.");

                return entities.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất danh sách dịch vụ y tế.", ex);
            }
        }

        public async Task<MedicalServiceResponseDTO?> GetMedicalServiceByIdAsync(int srvId)
        {
            if (srvId <= 0)
                throw new ArgumentException("ID dịch vụ y tế không hợp lệ.", nameof(srvId));

            try
            {
                var entity = await _medicalServiceRepo.GetMedicalServiceByIdAsync(srvId);
                if (entity == null)
                    throw new KeyNotFoundException($"Không tìm thấy dịch vụ y tế với ID {srvId}.");

                return MapToResponseDTO(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất dịch vụ y tế với ID {srvId}.", ex);
            }
        }

        public async Task<MedicalServiceResponseDTO> UpdateMedicalServiceAsync(int srvId, MedicalServiceRequestDTO medicalService)
        {
            if (srvId <= 0)
                throw new ArgumentException("ID dịch vụ y tế không hợp lệ.", nameof(srvId));

            if (medicalService == null)
                throw new ArgumentNullException(nameof(medicalService), "Yêu cầu DTO dịch vụ y tế không được để trống.");

            try
            {
                await ValidateAccountExists(medicalService.AccId);
                ValidateServiceName(medicalService.ServiceName);
                ValidatePrice(medicalService.Price);

                var entity = MapToEntity(medicalService);
                var updatedEntity = await _medicalServiceRepo.UpdateMedicalServiceAsync(srvId, entity);
                if (updatedEntity == null)
                    throw new KeyNotFoundException($"Không tìm thấy dịch vụ y tế với ID {srvId}.");

                return MapToResponseDTO(updatedEntity);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Lỗi cơ sở dữ liệu khi cập nhật dịch vụ y tế với ID {srvId}: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể cập nhật dịch vụ y tế với ID {srvId}.", ex);
            }
        }

        public async Task<MedicalServiceResponseDTO> DisableMedicalServiceAsync(int srvId)
        {
            if (srvId <= 0)
                throw new ArgumentException("ID dịch vụ y tế không hợp lệ.", nameof(srvId));

            try
            {
                var entity = await _medicalServiceRepo.DisableMedicalServiceAsync(srvId);
                if (entity == null)
                    throw new KeyNotFoundException($"Không tìm thấy dịch vụ y tế với ID {srvId} để vô hiệu hóa.");

                return MapToResponseDTO(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể vô hiệu hóa dịch vụ y tế với ID {srvId}.", ex);
            }
        }
    }
}