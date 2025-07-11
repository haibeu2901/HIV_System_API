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
    public class MedicationTemplateDAO : IMedicationTemplateDAO
    {
        private readonly HivSystemApiContext _context;
        private static MedicationTemplateDAO _instance;

        public MedicationTemplateDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static MedicationTemplateDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MedicationTemplateDAO();
                }
                return _instance;
            }
        }

        public async Task<ArvMedicationTemplate> CreateMedicationTemplateAsync(ArvMedicationTemplate medicationTemplate)
        {
            if (medicationTemplate == null)
                throw new ArgumentNullException(nameof(medicationTemplate));

            await _context.ArvMedicationTemplates.AddAsync(medicationTemplate);
            await _context.SaveChangesAsync();
            return medicationTemplate;
        }

        public async Task<bool> DeleteMedicationTemplateAsync(int id)
        {
            var template = await _context.ArvMedicationTemplates.FindAsync(id);
            if (template == null)
            {
                return false;
            }

            _context.ArvMedicationTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ArvMedicationTemplate>> GetAllMedicationTemplatesAsync()
        {
            return await _context.ArvMedicationTemplates.ToListAsync();
        }

        public async Task<ArvMedicationTemplate?> GetMedicationTemplateByIdAsync(int id)
        {
            return await _context.ArvMedicationTemplates
                .Include(t => t.Amd)
                .Include(t => t.Art)
                .FirstOrDefaultAsync(t => t.AmtId == id);
        }

        public async Task<List<ArvMedicationTemplate>> GetMedicationTemplatesByAmdIdAsync(int amdId)
        {
            return await _context.ArvMedicationTemplates
                .Include(t => t.Amd)
                .Include(t => t.Art)
                .Where(t => t.AmdId == amdId)
                .ToListAsync();
        }

        public async Task<List<ArvMedicationTemplate>> GetMedicationTemplatesByArtIdAsync(int artId)
        {
            return await _context.ArvMedicationTemplates
                .Include(t => t.Amd)
                .Include(t => t.Art)
                .Where(t => t.ArtId == artId)
                .ToListAsync();
        }

        public async Task<ArvMedicationTemplate> UpdateMedicationTemplateAsync(ArvMedicationTemplate medicationTemplate)
        {
            if (medicationTemplate == null)
                throw new ArgumentNullException(nameof(medicationTemplate));

            var existingTemplate = await _context.ArvMedicationTemplates
                .FirstOrDefaultAsync(t => t.AmtId == medicationTemplate.AmtId);

            if (existingTemplate == null)
                throw new KeyNotFoundException($"MedicationTemplate with AmtId {medicationTemplate.AmtId} not found.");

            existingTemplate.ArtId = medicationTemplate.ArtId;
            existingTemplate.AmdId = medicationTemplate.AmdId;
            existingTemplate.Quantity = medicationTemplate.Quantity;

            _context.ArvMedicationTemplates.Update(existingTemplate);
            await _context.SaveChangesAsync();

            return existingTemplate;
        }
    }
}
