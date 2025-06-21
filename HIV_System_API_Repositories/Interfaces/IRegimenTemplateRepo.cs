using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IRegimenTemplateRepo
    {
        Task<List<ArvRegimenTemplate>> GetAllRegimenTemplates();
        Task<ArvRegimenTemplate?> GetRegimenTemplateById(int id);
        Task<ArvRegimenTemplate?> CreateRegimenTemplate(ArvRegimenTemplate regimenTemplate);
        Task<ArvRegimenTemplate?> UpdateRegimenTemplate(int id, ArvRegimenTemplate regimenTemplate);
        Task<bool> DeleteRegimenTemplate(int id);
        Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByDescription(string description);
        Task<List<ArvRegimenTemplate>> GetRegimenTemplatesByLevel(byte level);
    }
}
