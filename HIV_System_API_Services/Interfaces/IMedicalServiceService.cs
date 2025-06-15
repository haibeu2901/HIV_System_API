using HIV_System_API_DTOs.MedicalServiceDTO;

namespace HIV_System_API_Services.Interfaces
{
    public interface IMedicalServiceService
    {
        Task<List<MedicalServiceResponseDTO>> GetAllMedicalServicesAsync();
        Task<MedicalServiceResponseDTO?> GetMedicalServiceByIdAsync(int srvId);
        Task<MedicalServiceResponseDTO> CreateMedicalServiceAsync(MedicalServiceRequestDTO medicalService);
        Task<MedicalServiceResponseDTO> UpdateMedicalServiceAsync(int srvId, MedicalServiceRequestDTO medicalService);
        Task<bool> DeleteMedicalServiceAsync(int srvId);
        Task<MedicalServiceResponseDTO> DisableMedicalServiceAsync(int srvId);
    }
}