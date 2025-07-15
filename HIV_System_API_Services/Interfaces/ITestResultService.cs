using HIV_System_API_BOs;
using HIV_System_API_DTOs.TestResultDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface ITestResultService
    {
        Task<List<TestResultResponseDTO>> GetAllTestResult();
        Task<TestResultResponseDTO> GetTestResultById(int id);
        Task<TestResultResponseDTO> CreateTestResult(TestResultRequestDTO testResult);
        Task<TestResultResponseDTO> UpdateTestResult(TestResultUpdateRequestDTO testResult, int id);
        Task<bool> DeleteTestResult(int id);
        Task<List<TestResultResponseDTO>> GetSustainTestResultPatient();
        Task<List<TestResultResponseDTO>> GetPersonalTestResult(int id);
    }
}
