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
    public class ArvMedicationDetailRepo : IArvMedicationDetailRepo
    {
        public async Task<List<ArvMedicationDetail>> GetAllArvMedicationDetailsAsync()
        {
            return await ArvMedicationDetailDAO.Instance.GetAllArvMedicationDetailsAsync();
        }

        public Task<ArvMedicationDetail> GetArvMedicationDetailByIdAsync(int id)
        {
            return ArvMedicationDetailDAO.Instance.GetArvMedicationDetailByIdAsync(id);
        }
    }
}
