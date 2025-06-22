using HIV_System_API_DTOs.ComponentTestResultDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IComponentTestResultService
    {
        Task<List<ComponentTestResultResponseDTO>> GetAllTestComponent();
        Task<ComponentTestResultResponseDTO> GetTestComponentById(int id);
        Task<ComponentTestResultResponseDTO> AddTestComponent(ComponentTestResultRequestDTO componentTestResult);
        Task<ComponentTestResultResponseDTO> UpdateTestComponent(int id, ComponentTestResultUpdateRequestDTO componentTestResult);
        Task <bool> DeleteTestComponent(int id);
    }
}
