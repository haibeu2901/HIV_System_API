using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_DAOs.Implements
{
    public class MedicalServiceDAO : IMedicalServiceDAO
    {
        private static MedicalServiceDAO? _instance;
        private readonly HivSystemContext _context;

        public static MedicalServiceDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MedicalServiceDAO();
                }
                return _instance;
            }
        }

        public MedicalServiceDAO()
        {
            _context = new HivSystemContext();
        }

        public async Task<List<MedicalService>> GetAllMedicalServicesAsync()
        {
            return await _context.MedicalServices.ToListAsync();
        }

        public async Task<MedicalService?> GetMedicalServiceByIdAsync(int srvId)
        {
            return await _context.MedicalServices.FindAsync(srvId);
        }

        public async Task<MedicalService> CreateMedicalServiceAsync(MedicalService medicalService)
        {
            if (medicalService == null)
                throw new ArgumentNullException(nameof(medicalService));

            await _context.MedicalServices.AddAsync(medicalService);
            await _context.SaveChangesAsync();
            return medicalService;
        }

        public async Task<MedicalService> UpdateMedicalServiceAsync(int srvId, MedicalService medicalService)
        {
            if (medicalService == null)
                throw new ArgumentNullException(nameof(medicalService));

            var existingService = await _context.MedicalServices.FindAsync(srvId);
            if (existingService == null)
                throw new KeyNotFoundException($"Medical Service with ID {srvId} not found.");

            existingService.AccId = medicalService.AccId;
            existingService.ServiceName = medicalService.ServiceName;
            existingService.ServiceDescription = medicalService.ServiceDescription;
            existingService.Price = medicalService.Price;
            existingService.IsAvailable = medicalService.IsAvailable;

            _context.MedicalServices.Update(existingService);
            await _context.SaveChangesAsync();
            return existingService;
        }

        public async Task<bool> DeleteMedicalServiceAsync(int srvId)
        {
            var service = await _context.MedicalServices.FindAsync(srvId);
            if (service == null)
                return false;

            _context.MedicalServices.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}