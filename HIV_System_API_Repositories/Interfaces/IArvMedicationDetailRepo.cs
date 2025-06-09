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
    }
}
