using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class PatientMedicalRecordDAO : IPatientMedicalRecordDAO
    {
        private readonly HivSystemApiContext _context;
        private static PatientMedicalRecordDAO? _instance;

        public PatientMedicalRecordDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static PatientMedicalRecordDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PatientMedicalRecordDAO();
                }
                return _instance;
            }
        }

        public async Task<List<PatientMedicalRecord>> GetAllPatientMedicalRecordsAsync()
        {
            return await _context.PatientMedicalRecords.ToListAsync();
        }

        public async Task<PatientMedicalRecord?> GetPatientMedicalRecordByIdAsync(int id)
        {
            return await _context.PatientMedicalRecords
                .FirstOrDefaultAsync(pmr => pmr.PmrId == id);
        }

        public async Task<PatientMedicalRecord> CreatePatientMedicalRecordAsync(PatientMedicalRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            await _context.PatientMedicalRecords.AddAsync(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<PatientMedicalRecord> UpdatePatientMedicalRecordAsync(int id, PatientMedicalRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var existingRecord = await _context.PatientMedicalRecords.FirstOrDefaultAsync(pmr => pmr.PmrId == id);
            if (existingRecord == null)
                throw new KeyNotFoundException($"PatientMedicalRecord with id {id} not found.");

            // Update properties
            existingRecord.PtnId = record.PtnId;
            // Add other property updates here as needed

            _context.PatientMedicalRecords.Update(existingRecord);
            await _context.SaveChangesAsync();
            return existingRecord;
        }

        public async Task<bool> DeletePatientMedicalRecordAsync(int id)
        {
            var record = await _context.PatientMedicalRecords.FirstOrDefaultAsync(pmr => pmr.PmrId == id);
            if (record == null)
                return false;

            _context.PatientMedicalRecords.Remove(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PatientMedicalRecord?> GetPersonalMedicalRecordAsync(int accId)
        {
            return await _context.PatientMedicalRecords
                .Include(pmr => pmr.Ptn)
                .FirstOrDefaultAsync(pmr => pmr.Ptn.AccId == accId);
        }

        public async Task<PatientMedicalRecord?> GetPatientMedicalRecordByPatientIdAsync(int patientId)
        {
            return await _context.PatientMedicalRecords
                .FirstOrDefaultAsync(pmr => pmr.PtnId == patientId);
        }
    }
}
