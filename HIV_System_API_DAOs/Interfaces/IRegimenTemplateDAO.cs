using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IRegimenTemplateDAO
    {
        Task<List<ArvRegimenTemplate>> GetAllRegimenTemplatesAsync();
        Task<ArvRegimenTemplate?> GetRegimenTemplateByIdAsync(int id);
        Task<ArvRegimenTemplate?> CreateRegimenTemplateAsync(ArvRegimenTemplate regimenTemplate);
        Task<ArvRegimenTemplate?> UpdateRegimenTemplateAsync(int id, ArvRegimenTemplate regimenTemplate);
        Task<bool> DeleteRegimenTemplateAsync(int id);
        Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByDescriptionAsync(string description);
        Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByLevelAsync(byte level);
    }
}
