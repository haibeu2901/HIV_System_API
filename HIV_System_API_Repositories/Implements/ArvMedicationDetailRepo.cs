using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DAOs.Interfaces;
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
        private readonly IArvMedicationDetailDAO _arvMedicationDetailDAO;

        public ArvMedicationDetailRepo()
        {
            _arvMedicationDetailDAO = new ArvMedicationDetailDAO();
        }

        // Use for unit testing or dependency injection
        public ArvMedicationDetailRepo(IArvMedicationDetailDAO arvMedicationDetailDAO)
        {
            _arvMedicationDetailDAO = arvMedicationDetailDAO;
        }

        public async Task<ArvMedicationDetail> CreateArvMedicationDetailAsync(ArvMedicationDetail arvMedicationDetail)
        {
            return await ArvMedicationDetailDAO.Instance.CreateArvMedicationDetailAsync(arvMedicationDetail);
        }

        public async Task<bool> DeleteArvMedicationDetailAsync(int id)
        {
            return await ArvMedicationDetailDAO.Instance.DeleteArvMedicationDetailAsync(id);
        }

        public async Task<List<ArvMedicationDetail>> GetAllArvMedicationDetailsAsync()
        {
            return await ArvMedicationDetailDAO.Instance.GetAllArvMedicationDetailsAsync();
        }

        public Task<ArvMedicationDetail> GetArvMedicationDetailByIdAsync(int id)
        {
            return ArvMedicationDetailDAO.Instance.GetArvMedicationDetailByIdAsync(id);
        }

        public async Task<List<ArvMedicationDetail>> SearchArvMedicationDetailsByNameAsync(string searchTerm)
        {
            return await ArvMedicationDetailDAO.Instance.SearchArvMedicationDetailsByNameAsync(searchTerm);
        }

        public async Task<ArvMedicationDetail> UpdateArvMedicationDetailAsync(int id, ArvMedicationDetail arvMedicationDetail)
        {
            return await ArvMedicationDetailDAO.Instance.UpdateArvMedicationDetailAsync(id, arvMedicationDetail);
        }
    }
}
