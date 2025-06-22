using HIV_System_API_BOs;
using HIV_System_API_DTOs.ComponentTestResultDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class ComponentTestResultService : IComponentTestResultService
    {
        private readonly IComponentTestResultRepo _componentTestResult;

        public ComponentTestResultService()
        {
                       _componentTestResult = new ComponentTestResultRepo();
        }

        private ComponentTestResult MapToRequest(ComponentTestResultRequestDTO componentTestResult)
        {
            return new ComponentTestResult
            {
                TrsId = componentTestResult.TestResultId,
                StfId = componentTestResult.StaffId,
                CtrName = componentTestResult.ComponentTestResultName,
                CtrDescription = componentTestResult.CtrDescription ?? string.Empty,
                ResultValue = componentTestResult.ResultValue,
                Notes = componentTestResult.Notes
            };
        }

        private ComponentTestResultResponseDTO MapToResponse(ComponentTestResult componentTestResult)
        {
            return new ComponentTestResultResponseDTO
            {
                ComponentTestResultId = componentTestResult.CtrId,
                TestResultId = componentTestResult.TrsId,
                StaffId = componentTestResult.StfId,
                ComponentTestResultName = componentTestResult.CtrName,
                CtrDescription = componentTestResult.CtrDescription ?? string.Empty,
                ResultValue = componentTestResult.ResultValue,
                Notes = componentTestResult.Notes
            };
        }
        public async Task<ComponentTestResultResponseDTO> AddTestComponent(ComponentTestResultRequestDTO componentTestResult)
        {
            var created = await _componentTestResult.AddTestComponent(MapToRequest(componentTestResult));
            if(created == null)
            {
                throw new Exception("Failed to add Test Component!");
            }
            return MapToResponse(created);
        }

        public Task<bool> DeleteTestComponent(int id)
        {
            if(id <=0)
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null.");
            }
            return _componentTestResult.DeleteTestComponent(id);
        }

        public async Task<List<ComponentTestResultResponseDTO>> GetAllTestComponent()
        {
            var components = await _componentTestResult.GetAllTestComponent();
            return components.Select(MapToResponse).ToList();
        }

        public async Task<ComponentTestResultResponseDTO> GetTestComponentById(int id)
        {
            var component = await _componentTestResult.GetTestComponentById(id);
            if (component == null)
            {
                throw new KeyNotFoundException($"Component with ID {id} not found.");
            }
            return MapToResponse(component);
        }

        public async Task<ComponentTestResultResponseDTO> UpdateTestComponent(int id, ComponentTestResultUpdateRequestDTO componentTestResult)
        {
            var existingComponent = await _componentTestResult.GetTestComponentById(id);
            if (existingComponent == null)
            {
                throw new KeyNotFoundException($"Component with ID {id} not found.");
            }
            if (id <= 0)
            {
                throw new ArgumentNullException(nameof(id), "ID cannot be null.");
            }
            if (componentTestResult == null)
            {
                throw new ArgumentNullException(nameof(componentTestResult), "Component test result cannot be null.");
            }
            var updatedComponent = new ComponentTestResult
            {
                Notes = componentTestResult.Notes,
                ResultValue = componentTestResult.ResultValue,
                CtrDescription = componentTestResult.CtrDescription,
                CtrName = componentTestResult.ComponentTestResultName,
            };

            var result = _componentTestResult.UpdateTestComponent(updatedComponent);
            if (!result.Result)
            {
                throw new Exception("Failed to update Test Component!");
            }
            return MapToResponse(updatedComponent);
        }
    }
}
