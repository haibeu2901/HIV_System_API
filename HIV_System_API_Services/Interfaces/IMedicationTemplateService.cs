using HIV_System_API_BOs;
using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IMedicationTemplateService
    {
        Task<List<MedicationTemplateResponseDTO>> GetAllMedicationTemplatesAsync();
        Task<MedicationTemplateResponseDTO?> GetMedicationTemplateByIdAsync(int id);
        Task<MedicationTemplateResponseDTO> CreateMedicationTemplateAsync(MedicationTemplateRequestDTO medicationTemplate);
        Task<MedicationTemplateResponseDTO> UpdateMedicationTemplateAsync(MedicationTemplateRequestDTO medicationTemplate);
        Task<bool> DeleteMedicationTemplateAsync(int id);
        Task<List<MedicationTemplateResponseDTO>> GetMedicationTemplatesByArtIdAsync(int artId);
        Task<List<MedicationTemplateResponseDTO>> GetMedicationTemplatesByAmdIdAsync(int amdId);
    }
}
