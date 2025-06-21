using HIV_System_API_BOs;
using HIV_System_API_DTOs.ComponentTestResultDTO;
using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Services.Interfaces;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Repositories.Implements;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_Services.Implements
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepo _testResultRepo;

        public TestResultService()
        {
            _testResultRepo = new TestResultRepo();
        }

        private TestResult MapToRequest(TestResultRequestDTO testResult)
        {
            return new TestResult
            {
                PmrId = testResult.PatientMedicalRecordId,
                TestDate = testResult.TestDate,
                ResultValue = testResult.Result,
                Notes = testResult.Notes
            };
        }

        private TestResultResponseDTO MapToResponse(TestResult testResult)
        {
            return new TestResultResponseDTO
            {
                PatientMedicalRecordId = testResult.PmrId,
                TestDate = testResult.TestDate,
                Result = testResult.ResultValue,
                Notes = testResult.Notes,
                TestResultId = testResult.TrsId,
                ComponentTestResults = testResult.ComponentTestResults?
                    .Select(component => new ComponentTestResultResponseDTO
                    {
                        ComponentTestResultId = component.CtrId,
                        TestResultId = component.TrsId,
                        StaffId = component.Stf.StfId,
                        ComponentTestResultName = component.CtrName,
                        CtrDescription = component.CtrDescription ?? string.Empty,
                        ResultValue = component.ResultValue,
                        Notes = component.Notes
                    }).ToList() ?? new List<ComponentTestResultResponseDTO>()
            };
        }

        public async Task<TestResultResponseDTO> CreateTestResult(TestResultRequestDTO testResult)
        {
            var entity = MapToRequest(testResult);
            var created = await _testResultRepo.CreateTestResult(entity);
            return MapToResponse(created);
        }

        public async Task<bool> DeleteTestResult(int id)
        {
            return await _testResultRepo.DeleteTestResult(id);
        }

        public async Task<List<TestResultResponseDTO>> GetAllTestResult()
        {
            // Ensure component test results are included
            var results = await _testResultRepo.GetAllTestResult();

            // If your repo does not include navigation properties, load them here:
            if (results.Any(r => r.ComponentTestResults == null || !r.ComponentTestResults.Any()))
            {
                using var context = new HivSystemApiContext();
                results = await context.TestResults
                    .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                    .ToListAsync();
            }

            return results.Select(MapToResponse).ToList();
        }
        public async Task<PersonalTestResultResponseDTO> GetPersonalTestResult(int id)
        {
            var testResult = await _testResultRepo.GetTestResultById(id);
            if (testResult == null)
                throw new KeyNotFoundException($"Test result with ID {id} not found.");

            return new PersonalTestResultResponseDTO
            {
                Result = testResult.ResultValue,
                TestDate = testResult.TestDate,
                Notes = testResult.Notes
            };
        }

        public async Task<List<TestResultResponseDTO>> GetPositiveTestResultPatient()
        {
            var results = await _testResultRepo.GetAllTestResult();
            
            // Filter for positive results where ResultValue is true
            var positiveResults = results.Where(r => r.ResultValue == true).ToList();
            return positiveResults.Select(MapToResponse).ToList();
        }

        public async Task<TestResultResponseDTO> GetTestResultById(int id)
        {
            // Ensure component test results are included
            var testResult = await _testResultRepo.GetTestResultById(id);

            // If your repo does not include navigation properties, load them here:
            if (testResult != null && (testResult.ComponentTestResults == null || !testResult.ComponentTestResults.Any()))
            {
                using var context = new HivSystemApiContext();
                testResult = await context.TestResults
                    .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                    .FirstOrDefaultAsync(tr => tr.TrsId == id);
            }

            if (testResult == null)
                throw new KeyNotFoundException($"Test result with ID {id} not found.");

            return MapToResponse(testResult);
        }

        public async Task<TestResultResponseDTO> UpdateTestResult(TestResultUpdateRequestDTO testResult, int id)
        {
            var existing = await _testResultRepo.GetTestResultById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Test result with ID {id} not found.");

            if (testResult.TestDate.HasValue)
                existing.TestDate = testResult.TestDate.Value;
            if (testResult.Result.HasValue)
                existing.ResultValue = testResult.Result.Value;
            if (testResult.Notes != null)
                existing.Notes = testResult.Notes;

            var updated = await _testResultRepo.UpdateTestResult(id, existing);
            if (!updated)
                throw new Exception("Update failed.");

            var updatedResult = await _testResultRepo.GetTestResultById(id);
            return MapToResponse(updatedResult!);
        }
    }
}
