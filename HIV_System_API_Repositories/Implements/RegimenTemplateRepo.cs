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
    public class RegimenTemplateRepo : IRegimenTemplateRepo
    {
        public async Task<ArvRegimenTemplate?> CreateRegimenTemplateAsync(ArvRegimenTemplate regimenTemplate)
        {
            return await RegimenTemplateDAO.Instance.CreateRegimenTemplateAsync(regimenTemplate);
        }

        public async Task<bool> DeleteRegimenTemplateAsync(int id)
        {
            return await RegimenTemplateDAO.Instance.DeleteRegimenTemplateAsync(id);
        }

        public async Task<List<ArvRegimenTemplate>> GetAllRegimenTemplatesAsync()
        {
            return await RegimenTemplateDAO.Instance.GetAllRegimenTemplatesAsync();
        }

        public async Task<ArvRegimenTemplate?> GetRegimenTemplateByIdAsync(int id)
        {
            return await RegimenTemplateDAO.Instance.GetRegimenTemplateByIdAsync(id);
        }

        public async Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByDescriptionAsync(string description)
        {
            return await RegimenTemplateDAO.Instance.GetRegimenTemplatesByDescriptionAsync(description);
        }

        public async Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByLevelAsync(byte level)
        {
            return await RegimenTemplateDAO.Instance.GetRegimenTemplatesByLevelAsync(level);
        }

        public async Task<ArvRegimenTemplate?> UpdateRegimenTemplateAsync(int id, ArvRegimenTemplate regimenTemplate)
        {
            return await RegimenTemplateDAO.Instance.UpdateRegimenTemplateAsync(id, regimenTemplate);
        }
    }
}
