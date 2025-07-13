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
    public class TestResultRepo : ITestResultRepo
    {
        public Task<TestResult> CreateTestResult(TestResult testResult)
        {
            return TestResultDAO.Instance.CreateTestResult(testResult);
        }

        public Task<bool> DeleteTestResult(int id)
        {
            return TestResultDAO.Instance.DeleteTestResult(id);
        }

        public Task<List<TestResult>> GetAllTestResult()
        {
            return TestResultDAO.Instance.GetAllTestResult();
        }

        public Task<TestResult?> GetTestResultById(int id)
        {
            return TestResultDAO.Instance.GetTestResultById(id);
        }

        public Task<bool> UpdateTestResult(int id, TestResult testResult)
        {
            return TestResultDAO.Instance.UpdateTestResult(id, testResult);
        }

        public Task<List<TestResult>> GetTestResultsByPatientId(int patientId)
        {
            return TestResultDAO.Instance.GetTestResultsByPatientId(patientId);
        }
    }
}
