using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IComponentTestResultDAO
    {
        Task<List<ComponentTestResult>> GetAllTestComponent();
        Task<ComponentTestResult> GetTestComponentById(int id);
        Task<ComponentTestResult> AddTestComponent(ComponentTestResult componentTestResult);
        Task<bool> UpdateTestComponent(ComponentTestResult componentTestResult);
        Task<bool> DeleteTestComponent(int id);
    }
}
