using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IArvMedicationDetailDAO
    {
        Task<List<ArvMedicationDetail>> GetAllArvMedicationDetailsAsync();
        Task<ArvMedicationDetail?> GetArvMedicationDetailByIdAsync(int id);
        Task<bool> CreateArvMedicationDetailAsync(ArvMedicationDetail arvMedicationDetail);
        Task<bool> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetail arvMedicationDetail);
        Task<bool> DeleteArvMedicationDetailAsync(int id);
        Task<List<ArvMedicationDetail>> SearchArvMedicationDetailsByNameAsync(string searchTerm);
    }
}
