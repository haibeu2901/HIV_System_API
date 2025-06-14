using HIV_System_API_BOs;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IMedicalServiceRepo
    {
        Task<List<MedicalService>> GetAllMedicalServicesAsync();
        Task<MedicalService?> GetMedicalServiceByIdAsync(int srvId);
        Task<MedicalService> CreateMedicalServiceAsync(MedicalService medicalService);
        Task<MedicalService> UpdateMedicalServiceAsync(int srvId, MedicalService medicalService);
        Task<bool> DeleteMedicalServiceAsync(int srvId);
    }
}