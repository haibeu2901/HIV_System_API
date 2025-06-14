using HIV_System_API_BOs;
using HIV_System_API_DTOs.MedicalServiceDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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
            var exists = await _context.Accounts.AnyAsync(acc => acc.AccId == accId);
            if (!exists)
            {
                throw new InvalidOperationException($"Account with ID {accId} does not exist.");
            }
        }

        private void ValidateServiceName(string serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                throw new ArgumentException("Service name cannot be empty or whitespace.");
            }
        }

        private void ValidatePrice(double price)
        {
            if (price < 0)
            {
                throw new ArgumentException("Price cannot be negative.");
            }
        }

        private MedicalService MapToEntity(MedicalServiceRequestDTO requestDTO)
        {
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
            return new MedicalServiceResponseDTO
            {
                SrvId = entity.SrvId,
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
                throw new ArgumentNullException(nameof(medicalService));

            try
            {
                await ValidateAccountExists(medicalService.AccId);
                ValidateServiceName(medicalService.ServiceName);
                ValidatePrice(medicalService.Price);

                var entity = MapToEntity(medicalService);
                var createdEntity = await _medicalServiceRepo.CreateMedicalServiceAsync(entity);
                return MapToResponseDTO(createdEntity);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while creating medical service: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error creating medical service: {ex.Message}");
            }
        }

        public async Task<bool> DeleteMedicalServiceAsync(int srvId)
        {
            return await _medicalServiceRepo.DeleteMedicalServiceAsync(srvId);
        }

        public async Task<List<MedicalServiceResponseDTO>> GetAllMedicalServicesAsync()
        {
            var entities = await _medicalServiceRepo.GetAllMedicalServicesAsync();
            return entities.Select(MapToResponseDTO).ToList();
        }

        public async Task<MedicalServiceResponseDTO?> GetMedicalServiceByIdAsync(int srvId)
        {
            var entity = await _medicalServiceRepo.GetMedicalServiceByIdAsync(srvId);
            if (entity == null)
                return null;
            return MapToResponseDTO(entity);
        }

        public async Task<MedicalServiceResponseDTO> UpdateMedicalServiceAsync(int srvId, MedicalServiceRequestDTO medicalService)
        {
            if (medicalService == null)
                throw new ArgumentNullException(nameof(medicalService));

            try
            {
                await ValidateAccountExists(medicalService.AccId);
                ValidateServiceName(medicalService.ServiceName);
                ValidatePrice(medicalService.Price);

                var entity = MapToEntity(medicalService);
                var updatedEntity = await _medicalServiceRepo.UpdateMedicalServiceAsync(srvId, entity);
                return MapToResponseDTO(updatedEntity);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error while updating medical service: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unexpected error updating medical service: {ex.Message}");
            }
        }
    }
}