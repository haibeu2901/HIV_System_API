using HIV_System_API_BOs;
using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
using HIV_System_API_DTOs.ARVRegimenTemplateDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IRegimenTemplateService
    {
        Task<List<RegimenTemplateResponseDTO>> GetAllRegimenTemplatesAsync();
        Task<RegimenTemplateResponseDTO?> GetRegimenTemplateByIdAsync(int id);
        Task<RegimenTemplateResponseDTO?> CreateRegimenTemplateAsync(RegimenTemplateRequestDTO regimenTemplate);
        Task<RegimenTemplateResponseDTO?> UpdateRegimenTemplateAsync(int id, RegimenTemplateRequestDTO regimenTemplate);
        Task<bool> DeleteRegimenTemplateAsync(int id);
        Task<List<RegimenTemplateResponseDTO>> GetRegimenTemplatesByDescriptionAsync(string description);
        Task<List<RegimenTemplateResponseDTO>> GetRegimenTemplatesByLevelAsync(byte level);
        Task<RegimenTemplateResponseDTO> CreateRegimenTemplateWithMedicationsTemplate(RegimenTemplateRequestDTO regimenTemplate, List<MedicationTemplateRequestDTO> medicationTemplates);
        //Task<RegimenTemplateResponseDTO> UpdateRegimenTemplateWithMedicationsTemplate(int id, RegimenTemplateRequestDTO regimenTemplate, List<MedicationTemplateRequestDTO> medicationTemplates, int accId);
    }
}
