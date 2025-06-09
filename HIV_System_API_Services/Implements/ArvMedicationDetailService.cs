using HIV_System_API_BOs;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class ArvMedicationDetailService : IArvMedicationDetailService
    {
        private readonly IArvMedicationDetailRepo _arvMedicationDetailRepo;
        public ArvMedicationDetailService()
        {
            _arvMedicationDetailRepo = new ArvMedicationDetailRepo();
        }
        public async Task<List<ArvMedicationDetail>> GetAllArvMedicationDetailsAsync()
        {
            return await _arvMedicationDetailRepo.GetAllArvMedicationDetailsAsync();
        }

        public async Task<ArvMedicationDetail> GetArvMedicationDetailByIdAsync(int id)
        {
            return await _arvMedicationDetailRepo.GetArvMedicationDetailByIdAsync(id);
        }
    }
}
