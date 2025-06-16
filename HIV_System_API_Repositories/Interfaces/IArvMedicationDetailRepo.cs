using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface IArvMedicationDetailRepo
    {
        Task<List<ArvMedicationDetail>> GetAllArvMedicationDetailsAsync();
        Task<ArvMedicationDetail> GetArvMedicationDetailByIdAsync(int id);
        Task<ArvMedicationDetail> CreateArvMedicationDetailAsync(ArvMedicationDetail arvMedicationDetail);
        Task<ArvMedicationDetail> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetail arvMedicationDetail);
        Task<bool> DeleteArvMedicationDetailAsync(int id);
        Task<List<ArvMedicationDetail>> SearchArvMedicationDetailsByNameAsync(string searchTerm);
    }
}
