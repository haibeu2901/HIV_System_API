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
    public class RegimenTemplateDAO : IRegimenTemplateDAO
    {
        private readonly HivSystemApiContext _context;
        private static RegimenTemplateDAO _instance;

        public RegimenTemplateDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static RegimenTemplateDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RegimenTemplateDAO();
                }
                return _instance;
            }
        }

        public async Task<ArvRegimenTemplate?> CreateRegimenTemplateAsync(ArvRegimenTemplate regimenTemplate)
        {
            if (regimenTemplate == null)
            {
                return null;
            }

            await _context.ArvRegimenTemplates.AddAsync(regimenTemplate);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                return await _context.ArvRegimenTemplates
                    .Include(r => r.ArvMedicationTemplates)
                    .FirstOrDefaultAsync(r => r.ArtId == regimenTemplate.ArtId);
            }
            return null;
        }

        public async Task<bool> DeleteRegimenTemplateAsync(int id)
        {
            var regimenTemplate = await _context.ArvRegimenTemplates.FindAsync(id);
            if (regimenTemplate == null)
            {
                return false; // Not found
            }
            _context.ArvRegimenTemplates.Remove(regimenTemplate);
            return await _context.SaveChangesAsync() > 0; // Returns true if the deletion was successful
        }

        public async Task<List<ArvRegimenTemplate>> GetAllRegimenTemplatesAsync()
        {
            return await _context.ArvRegimenTemplates
                .Include(r => r.ArvMedicationTemplates)
                .ToListAsync();
        }

        public async Task<ArvRegimenTemplate?> GetRegimenTemplateByIdAsync(int id)
        {
            return await _context.ArvRegimenTemplates
                .Include(r => r.ArvMedicationTemplates)
                .FirstOrDefaultAsync(r => r.ArtId == id);
        }

        public async Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByDescriptionAsync(string description)
        {
            return await _context.ArvRegimenTemplates
                .Where(r => r.Description != null && r.Description.Contains(description))
                .Include(r => r.ArvMedicationTemplates)
                .ToListAsync();
        }

        public async Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByLevelAsync(byte level)
        {
            return await _context.ArvRegimenTemplates
                .Where(r => r.Level == level)
                .Include(r => r.ArvMedicationTemplates)
                .ToListAsync();
        }

        public async Task<ArvRegimenTemplate?> UpdateRegimenTemplateAsync(int id, ArvRegimenTemplate regimenTemplate)
        {
            if (regimenTemplate == null)
            {
                return null;
            }

            var existing = await _context.ArvRegimenTemplates
                .Include(r => r.ArvMedicationTemplates)
                .FirstOrDefaultAsync(r => r.ArtId == id);

            if (existing == null)
            {
                return null;
            }

            // Update scalar properties
            existing.Description = regimenTemplate.Description;
            existing.Level = regimenTemplate.Level;
            existing.Duration = regimenTemplate.Duration;

            // Update ArvMedicationTemplates
            if (regimenTemplate.ArvMedicationTemplates != null)
            {
                // Remove medications not in the new list
                var toRemove = existing.ArvMedicationTemplates
                    .Where(em => !regimenTemplate.ArvMedicationTemplates.Any(nm => nm.AmtId == em.AmtId))
                    .ToList();
                foreach (var med in toRemove)
                {
                    _context.ArvMedicationTemplates.Remove(med);
                }

                // Update or add medications
                foreach (var newMed in regimenTemplate.ArvMedicationTemplates)
                {
                    var existingMed = existing.ArvMedicationTemplates.FirstOrDefault(em => em.AmtId == newMed.AmtId);
                    if (existingMed != null)
                    {
                        existingMed.AmdId = newMed.AmdId;
                        existingMed.Quantity = newMed.Quantity;
                    }
                    else
                    {
                        var medToAdd = new ArvMedicationTemplate
                        {
                            ArtId = existing.ArtId,
                            AmdId = newMed.AmdId,
                            Quantity = newMed.Quantity
                        };
                        existing.ArvMedicationTemplates.Add(medToAdd);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return await _context.ArvRegimenTemplates
                .Include(r => r.ArvMedicationTemplates)
                .FirstOrDefaultAsync(r => r.ArtId == id);
        }
    }
}
