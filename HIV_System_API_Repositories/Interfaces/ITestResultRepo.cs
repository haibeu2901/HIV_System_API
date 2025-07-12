using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface ITestResultRepo
    {
        Task<List<TestResult>> GetAllTestResult();
        Task<TestResult?> GetTestResultById(int id);
        Task<TestResult> CreateTestResult(TestResult testResult);
        Task<bool> UpdateTestResult(int id, TestResult testResult);
        Task<bool> DeleteTestResult(int id);
        Task<List<TestResult>> GetTestResultsByPatientId(int patientId);
    }
}
