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
            if (componentTestResult == null)
                throw new ArgumentNullException(nameof(componentTestResult), "Yêu cầu DTO kết quả xét nghiệm không được để trống.");

            if (componentTestResult.TestResultId <= 0)
                throw new ArgumentException("ID kết quả xét nghiệm không hợp lệ.", nameof(componentTestResult.TestResultId));

            if (componentTestResult.StaffId <= 0)
                throw new ArgumentException("ID nhân viên không hợp lệ.", nameof(componentTestResult.StaffId));

            if (string.IsNullOrWhiteSpace(componentTestResult.ComponentTestResultName))
                throw new ArgumentException("Tên kết quả xét nghiệm không được để trống.", nameof(componentTestResult.ComponentTestResultName));

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
            if (componentTestResult == null)
                throw new ArgumentNullException(nameof(componentTestResult), "Thực thể kết quả xét nghiệm không được để trống.");

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
            if (componentTestResult == null)
                throw new ArgumentNullException(nameof(componentTestResult), "Yêu cầu DTO kết quả xét nghiệm không được để trống.");

            try
            {
                var created = await _componentTestResult.AddTestComponent(MapToRequest(componentTestResult));
                if (created == null)
                    throw new InvalidOperationException("Không thể thêm thành phần xét nghiệm.");

                return MapToResponse(created);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể thêm thành phần xét nghiệm.", ex);
            }
        }

        public async Task<bool> DeleteTestComponent(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID thành phần xét nghiệm không hợp lệ.", nameof(id));

            try
            {
                var result = await _componentTestResult.DeleteTestComponent(id);
                if (!result)
                    throw new InvalidOperationException($"Không tìm thấy thành phần xét nghiệm với ID {id} để xóa.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể xóa thành phần xét nghiệm với ID {id}.", ex);
            }
        }

        public async Task<List<ComponentTestResultResponseDTO>> GetAllTestComponent()
        {
            try
            {
                var components = await _componentTestResult.GetAllTestComponent();
                return components.Select(MapToResponse).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể truy xuất danh sách thành phần xét nghiệm.", ex);
            }
        }

        public async Task<ComponentTestResultResponseDTO> GetTestComponentById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID thành phần xét nghiệm không hợp lệ.", nameof(id));

            try
            {
                var component = await _componentTestResult.GetTestComponentById(id);
                if (component == null)
                    throw new KeyNotFoundException($"Không tìm thấy thành phần xét nghiệm với ID {id}.");

                return MapToResponse(component);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất thành phần xét nghiệm với ID {id}.", ex);
            }
        }

        public async Task<ComponentTestResultResponseDTO> UpdateTestComponent(int id, ComponentTestResultUpdateRequestDTO componentTestResult)
        {
            if (id <= 0)
                throw new ArgumentException("ID thành phần xét nghiệm không hợp lệ.", nameof(id));

            if (componentTestResult == null)
                throw new ArgumentNullException(nameof(componentTestResult), "Yêu cầu DTO cập nhật kết quả xét nghiệm không được để trống.");

            if (string.IsNullOrWhiteSpace(componentTestResult.ComponentTestResultName))
                throw new ArgumentException("Tên kết quả xét nghiệm không được để trống.", nameof(componentTestResult.ComponentTestResultName));

            try
            {
                var existingComponent = await _componentTestResult.GetTestComponentById(id);
                if (existingComponent == null)
                    throw new KeyNotFoundException($"Không tìm thấy thành phần xét nghiệm với ID {id}.");

                var updatedComponent = new ComponentTestResult
                {
                    CtrId = id,
                    TrsId = existingComponent.TrsId,
                    StfId = existingComponent.StfId,
                    CtrName = componentTestResult.ComponentTestResultName,
                    CtrDescription = componentTestResult.CtrDescription ?? string.Empty,
                    ResultValue = componentTestResult.ResultValue,
                    Notes = componentTestResult.Notes
                };

                var result = await _componentTestResult.UpdateTestComponent(updatedComponent);
                if (!result)
                    throw new InvalidOperationException($"Không thể cập nhật thành phần xét nghiệm với ID {id}.");

                return MapToResponse(updatedComponent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể cập nhật thành phần xét nghiệm với ID {id}.", ex);
            }
        }
    }
}