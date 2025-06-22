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
    public class ComponentTestResultRepo : IComponentTestResultRepo
    {
        public async Task<ComponentTestResult> AddTestComponent(ComponentTestResult componentTestResult)
        {
            return await ComponentTestResultDAO.Instance.AddTestComponent(componentTestResult);
        }

        public async Task<bool> DeleteTestComponent(int id)
        {
            return await ComponentTestResultDAO.Instance.DeleteTestComponent(id);
        }

        public async Task<List<ComponentTestResult>> GetAllTestComponent()
        {
            return await ComponentTestResultDAO.Instance.GetAllTestComponent();
        }

        public async Task<ComponentTestResult> GetTestComponentById(int id)
        {
            return await ComponentTestResultDAO.Instance.GetTestComponentById(id);
        }

        public Task<bool> UpdateTestComponent(ComponentTestResult componentTestResult)
        {
            return ComponentTestResultDAO.Instance.UpdateTestComponent(componentTestResult);
        }
    }
}
