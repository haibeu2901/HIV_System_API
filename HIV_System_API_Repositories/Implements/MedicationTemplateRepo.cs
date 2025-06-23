using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class MedicationTemplateRepo : IMedicationTemplateRepo
    {
        public async Task<ArvMedicationTemplate> CreateMedicationTemplateAsync(ArvMedicationTemplate medicationTemplate)
        {
            return await MedicationTemplateDAO.Instance.CreateMedicationTemplateAsync(medicationTemplate);
        }

        public async Task<bool> DeleteMedicationTemplateAsync(int id)
        {
            return await MedicationTemplateDAO.Instance.DeleteMedicationTemplateAsync(id);
        }

        public async Task<List<ArvMedicationTemplate>> GetAllMedicationTemplatesAsync()
        {
            return await MedicationTemplateDAO.Instance.GetAllMedicationTemplatesAsync();
        }

        public async Task<ArvMedicationTemplate?> GetMedicationTemplateByIdAsync(int id)
        {
            return await MedicationTemplateDAO.Instance.GetMedicationTemplateByIdAsync(id);
        }

        public async Task<List<ArvMedicationTemplate>> GetMedicationTemplatesByAmdIdAsync(int amdId)
        {
            return await MedicationTemplateDAO.Instance.GetMedicationTemplatesByAmdIdAsync(amdId);
        }

        public async Task<List<ArvMedicationTemplate>> GetMedicationTemplatesByArtIdAsync(int artId)
        {
            return await MedicationTemplateDAO.Instance.GetMedicationTemplatesByArtIdAsync(artId);
        }

        public async Task<ArvMedicationTemplate> UpdateMedicationTemplateAsync(ArvMedicationTemplate medicationTemplate)
        {
            return await MedicationTemplateDAO.Instance.UpdateMedicationTemplateAsync(medicationTemplate);
        }
    }
}
