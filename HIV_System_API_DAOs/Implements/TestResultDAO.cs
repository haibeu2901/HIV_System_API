using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class TestResultDAO : ITestResultDAO
    {
        private readonly HivSystemApiContext _context;
        private static TestResultDAO? _instance;

        public TestResultDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static TestResultDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TestResultDAO();
                }
                return _instance;
            }
        }


        public async Task<TestResult> CreateTestResult(TestResult testResult)
        {
            if (testResult == null)
            {
                throw new ArgumentNullException(nameof(testResult), "Test Result cannot be null");
            }
            await _context.TestResults.AddAsync(testResult);
            _context.SaveChanges();
            return testResult;
        }

        public async Task<bool> DeleteTestResult(int id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null)
            {
                throw new ArgumentException("Test Result not found", nameof(id));
            }
            _context.TestResults.Remove(testResult);
            _context.SaveChanges();
            return true;
        }

        public async Task<List<TestResult>> GetAllTestResult()
        {
            return await _context.TestResults.ToListAsync();
        }


        public async Task<TestResult?> GetTestResultById(int id)
        {
            var testResult = await _context.TestResults.FindAsync(id);
            if (testResult == null)
            {
                throw new Exception($"Test Result with ID {id} not found.");
            }
            return testResult;
        }

        public async Task<bool> UpdateTestResult(int id, TestResult testResult)
        {
            var existingTestResult = await _context.TestResults.FirstOrDefaultAsync(tr => tr.TrsId == id);
            if (existingTestResult == null)
            {
                throw new ArgumentException("Test Result not found", nameof(id));
            }

            existingTestResult.PmrId = testResult.PmrId;
            existingTestResult.TestDate = testResult.TestDate;
            existingTestResult.ResultValue = testResult.ResultValue;
            existingTestResult.Notes = testResult.Notes;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<TestResult>> GetTestResultsByPatientId(int patientId)
        {
            return await _context.TestResults
                .Where(tr => tr.PmrId == patientId)
                .ToListAsync();
        }
    }
}
