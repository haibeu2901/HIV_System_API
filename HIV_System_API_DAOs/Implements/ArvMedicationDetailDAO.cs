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
        private readonly HivSystemContext _context;
        private static ArvMedicationDetailDAO _instance;

        public ArvMedicationDetailDAO()
        {
            _context = new HivSystemContext();
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
    }
}
