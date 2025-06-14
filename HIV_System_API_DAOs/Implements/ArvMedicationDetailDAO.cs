using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class ArvMedicationDetailDAO : IArvMedicationDetailDAO
    {
        private readonly HivSystemApiContext _context;
        private static ArvMedicationDetailDAO _instance;

        public ArvMedicationDetailDAO()
        {
            _context = new HivSystemApiContext();
        }
        public static ArvMedicationDetailDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ArvMedicationDetailDAO();
                }
                return _instance;
            }
        }

        public async Task<List<ArvMedicationDetail>> GetAllArvMedicationDetailsAsync()
        {
            return await _context.ArvMedicationDetails.ToListAsync();
        }

        public async Task<ArvMedicationDetail> GetArvMedicationDetailByIdAsync(int id)
        {
            // Validate input parameter
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than zero.");
            }
            // Query the database asynchronously to find the ARV medication detail by ID
            var arvMedicationDetail = await _context.ArvMedicationDetails
                .FirstOrDefaultAsync(a => a.AmdId == id);
            // Return the ARV medication detail or null if not found
            return arvMedicationDetail;
        }

        public async Task<bool> CreateArvMedicationDetailAsync(ArvMedicationDetail arvMedicationDetail)
        {
            // Validate input parameter
            if (arvMedicationDetail == null)
            {
                throw new ArgumentNullException(nameof(arvMedicationDetail), "ARV medication detail cannot be null.");
            }
            // Add the new ARV medication detail to the context
            await _context.ArvMedicationDetails.AddAsync(arvMedicationDetail);
            // Save changes to the database and return the result
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetail arvMedicationDetail)
        {
            // Validate input parameters
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than zero.");
            }
            if (arvMedicationDetail == null)
            {
                throw new ArgumentNullException(nameof(arvMedicationDetail), "ARV medication detail cannot be null.");
            }
            // Find the existing ARV medication detail by ID
            var existingDetail = await _context.ArvMedicationDetails.FindAsync(id);
            if (existingDetail == null)
            {
                return false; // Not found
            }
            // Update the properties of the existing detail
            existingDetail.AmdId = id; // Ensure ID remains the same
            existingDetail.MedName = arvMedicationDetail.MedName;
            existingDetail.MedDescription = arvMedicationDetail.MedDescription;
            existingDetail.Dosage = arvMedicationDetail.Dosage;
            existingDetail.Price = arvMedicationDetail.Price;
            existingDetail.Manufactorer = arvMedicationDetail.Manufactorer;
            // Save changes to the database and return the result
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteArvMedicationDetailAsync(int id)
        {
            // Validate input parameter
            if (id <= 0)
            {
                throw new ArgumentException("ID must be greater than zero.");
            }
            // Find the ARV medication detail by ID
            var arvMedicationDetail = await _context.ArvMedicationDetails.FindAsync(id);
            if (arvMedicationDetail == null)
            {
                return false; // Not found
            }
            // Remove the ARV medication detail from the context
            _context.ArvMedicationDetails.Remove(arvMedicationDetail);
            // Save changes to the database and return the result
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ArvMedicationDetail>> SearchArvMedicationDetailsByNameAsync(string searchTerm)
        {
            // Validate input parameter
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                throw new ArgumentException("Search term cannot be null or empty.", nameof(searchTerm));
            }
            // Query the database asynchronously to find ARV medication details by name
            return await _context.ArvMedicationDetails
                .Where(a => a.MedName.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }
    }
}
