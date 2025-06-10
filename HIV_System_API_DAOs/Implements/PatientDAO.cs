using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class PatientDAO : IPatientDAO
    {
        private readonly HivSystemContext _context;
        private static PatientDAO _instance;

        public PatientDAO()
        {
            _context = new HivSystemContext();
        }

        public static PatientDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PatientDAO();
                }
                return _instance;
            }
        }

        public async Task<List<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients.ToListAsync();
        }

        public Task<Patient> GetPatientByIdAsync(int patientId)
        {
            return _context.Patients.FirstOrDefaultAsync(p => p.PtnId == patientId);
        }

        public Task<List<Patient>> GetPatientsByNameAsync(string name)
        {
            return _context.Patients
                .Include(p => p.Acc)
                .Where(p => p.Acc.Fullname != null &&
                    EF.Functions.Like(p.Acc.Fullname.ToLower(), $"%{name.ToLower()}%"))
                .ToListAsync();
        }
    }
}
