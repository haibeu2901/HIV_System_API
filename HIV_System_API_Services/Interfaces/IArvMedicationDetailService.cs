using HIV_System_API_BOs;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IArvMedicationDetailService
    {
        Task<List<ArvMedicationDetailDTO>> GetAllArvMedicationDetailsAsync();
        Task<ArvMedicationDetailDTO> GetArvMedicationDetailByIdAsync(int id);
        Task<ArvMedicationDetailDTO> CreateArvMedicationDetailAsync(ArvMedicationDetailDTO arvMedicationDetail);
        Task<ArvMedicationDetailDTO> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetailDTO arvMedicationDetail);
        Task<bool> DeleteArvMedicationDetailAsync(int id);
        Task<List<ArvMedicationDetailDTO>> SearchArvMedicationDetailsByNameAsync(string searchTerm);
    }
}
