using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;

namespace HIV_System_API_Repositories.Implements
{
    public class MedicalServiceRepo : IMedicalServiceRepo
    {
        public async Task<List<MedicalService>> GetAllMedicalServicesAsync()
        {
            return await MedicalServiceDAO.Instance.GetAllMedicalServicesAsync();
        }

        public async Task<MedicalService?> GetMedicalServiceByIdAsync(int srvId)
        {
            return await MedicalServiceDAO.Instance.GetMedicalServiceByIdAsync(srvId);
        }

        public async Task<MedicalService> CreateMedicalServiceAsync(MedicalService medicalService)
        {
            return await MedicalServiceDAO.Instance.CreateMedicalServiceAsync(medicalService);
        }

        public async Task<MedicalService> UpdateMedicalServiceAsync(int srvId, MedicalService medicalService)
        {
            return await MedicalServiceDAO.Instance.UpdateMedicalServiceAsync(srvId, medicalService);
        }

        public async Task<bool> DeleteMedicalServiceAsync(int srvId)
        {
            return await MedicalServiceDAO.Instance.DeleteMedicalServiceAsync(srvId);
        }
    }
}