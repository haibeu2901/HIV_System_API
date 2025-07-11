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
        Task<List<ArvMedicationDetailResponseDTO>> GetAllArvMedicationDetailsAsync();
        Task<ArvMedicationDetailResponseDTO> GetArvMedicationDetailByIdAsync(int id);
        Task<ArvMedicationDetailResponseDTO> CreateArvMedicationDetailAsync(ArvMedicationDetailResponseDTO arvMedicationDetail);
        Task<ArvMedicationDetailResponseDTO> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetailResponseDTO arvMedicationDetail);
        Task<bool> DeleteArvMedicationDetailAsync(int id);
        Task<List<ArvMedicationDetailResponseDTO>> SearchArvMedicationDetailsByNameAsync(string searchTerm);
    }
}
