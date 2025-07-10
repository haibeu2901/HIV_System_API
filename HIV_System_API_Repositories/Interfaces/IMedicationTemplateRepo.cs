using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IMedicationTemplateRepo
    {
        Task<List<ArvMedicationTemplate>> GetAllMedicationTemplatesAsync();
        Task<ArvMedicationTemplate?> GetMedicationTemplateByIdAsync(int id);
        Task<ArvMedicationTemplate> CreateMedicationTemplateAsync(ArvMedicationTemplate medicationTemplate);
        Task<ArvMedicationTemplate> UpdateMedicationTemplateAsync(ArvMedicationTemplate medicationTemplate);
        Task<bool> DeleteMedicationTemplateAsync(int id);
        Task<List<ArvMedicationTemplate>> GetMedicationTemplatesByArtIdAsync(int artId);
        Task<List<ArvMedicationTemplate>> GetMedicationTemplatesByAmdIdAsync(int amdId);
    }
}
