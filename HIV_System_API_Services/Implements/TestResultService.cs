using HIV_System_API_BOs;
using HIV_System_API_DTOs.ComponentTestResultDTO;
using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;

namespace HIV_System_API_Services.Implements
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepo _testResultRepo;
        private readonly IComponentTestResultRepo _componentTestResultRepo;
        private readonly IPatientRepo _patientRepo;
        private readonly INotificationRepo _notificationRepo;
        private readonly HivSystemApiContext _context;

        public TestResultService()
        {
            _testResultRepo = new TestResultRepo() ?? throw new ArgumentNullException(nameof(_testResultRepo));
            _componentTestResultRepo = new ComponentTestResultRepo() ?? throw new ArgumentNullException(nameof(_componentTestResultRepo));
            _patientRepo = new PatientRepo() ?? throw new ArgumentNullException(nameof(_patientRepo));
            _notificationRepo = new NotificationRepo() ?? throw new ArgumentNullException(nameof(_notificationRepo));
            _context = new HivSystemApiContext() ?? throw new ArgumentNullException(nameof(_context));
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
                    .OrderBy(ct => ct.CtrName) // Optional: Order component test results by name
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

        private async Task ValidatePatientMedicalRecordExists(int pmrId)
        {
            var exists = await _context.PatientMedicalRecords.AnyAsync(pmr => pmr.PmrId == pmrId);
            if (!exists)
            {
                throw new InvalidOperationException($"Hồ sơ bệnh án với ID {pmrId} không tồn tại.");
            }
        }

        private async Task ValidateStaffExists(int staffId)
        {
            var exists = await _context.Staff.AnyAsync(s => s.StfId == staffId);
            if (!exists)
            {
                throw new InvalidOperationException($"Nhân viên với ID {staffId} không tồn tại.");
            }
        }

        private async Task ValidateAccountExists(int accId)
        {
            if (accId <= 0)
                throw new ArgumentException("Invalid Account ID", nameof(accId));
            var exists = await _context.Accounts.AnyAsync(a => a.AccId == accId);
            if (!exists)
                throw new InvalidOperationException($"Tài khoản với ID {accId} không tồn tại.");
        }

        public async Task<TestResultResponseDTO> CreateTestResult(TestResultRequestDTO testResult)
        {
            if (testResult == null)
                throw new ArgumentNullException(nameof(testResult));

            await ValidatePatientMedicalRecordExists(testResult.PatientMedicalRecordId);

            var entity = MapToRequest(testResult);
            var created = await _testResultRepo.CreateTestResult(entity);
            return MapToResponse(created);
        }

        public async Task<bool> DeleteTestResult(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID kết quả xét nghiệm không hợp lệ", nameof(id));

            var existing = await _testResultRepo.GetTestResultById(id);
            if (existing == null)
                throw new InvalidOperationException($"Kết quả xét nghiệm với ID {id} không tồn tại.");

            return await _testResultRepo.DeleteTestResult(id);
        }

        public async Task<List<TestResultResponseDTO>> GetAllTestResult()
        {
            // Ensure component test results are included and ordered by test date descending
            var results = await _testResultRepo.GetAllTestResult();

            // If your repo does not include navigation properties, load them here:
            if (results.Any(r => r.ComponentTestResults == null || !r.ComponentTestResults.Any()))
            {
                using var context = new HivSystemApiContext();
                results = await context.TestResults
                    .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                    .OrderByDescending(tr => tr.TestDate) // Order by TestDate descending
                    .ThenByDescending(tr => tr.TrsId)
                    .ToListAsync();
            }
            else
            {
                // Order the results from repository as well
                results = results.OrderByDescending(r => r.TestDate).ToList();
            }

            return results.Select(MapToResponse).ToList();
        }

        public async Task<List<PersonalTestResultResponseDTO>> GetPersonalTestResult(int id)
        {
            var patient = await _patientRepo.GetPatientByIdAsync(id);
            if (patient == null)
                throw new KeyNotFoundException($"Bệnh nhân với ID {id} không tìm thấy.");

            var results = await _testResultRepo.GetTestResultsByPatientId(patient.PatientMedicalRecord.PmrId);
            if (results == null || !results.Any())
                throw new KeyNotFoundException($"Không tìm thấy kết quả xét nghiệm cho bệnh nhân với ID {id}.");

            // Order by TestDate descending to show most recent first
            var orderedResults = results.OrderByDescending(r => r.TestDate).ThenByDescending(r => r.TrsId);

            var response = orderedResults.Select(r => new PersonalTestResultResponseDTO
            {
                PatientMedicalRecordId = r.PmrId,
                TestDate = r.TestDate,
                Result = r.ResultValue,
                Notes = r.Notes,
                ComponentTestResults = r.ComponentTestResults?
                    .Select(ct => new ComponentTestResultResponseDTO
                    {
                        ComponentTestResultId = ct.CtrId,
                        TestResultId = ct.TrsId,
                        StaffId = ct.Stf.StfId,
                        ComponentTestResultName = ct.CtrName,
                        CtrDescription = ct.CtrDescription ?? string.Empty,
                        ResultValue = ct.ResultValue,
                        Notes = ct.Notes
                    }).ToList() ?? new List<ComponentTestResultResponseDTO>()
            }).ToList();

            return response;
        }

        public async Task<List<TestResultResponseDTO>> GetSustainTestResultPatient()
        {
            using var context = new HivSystemApiContext();
            var positiveResults = await context.TestResults
                .Include(tr => tr.ComponentTestResults)
                .ThenInclude(ct => ct.Stf)
                .Where(r => r.ResultValue == true)
                .OrderByDescending(r => r.TestDate) // Order by TestDate descending
                .ThenByDescending(r => r.TrsId)
                .ToListAsync();

            return positiveResults.Select(MapToResponse).ToList();
        }

        public async Task<TestResultResponseDTO> GetTestResultById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID kết quả xét nghiệm không hợp lệ", nameof(id));

            var testResult = await _testResultRepo.GetTestResultById(id);

            if (testResult != null && (testResult.ComponentTestResults == null || !testResult.ComponentTestResults.Any()))
            {
                using var context = new HivSystemApiContext();
                testResult = await context.TestResults
                    .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                    .FirstOrDefaultAsync(tr => tr.TrsId == id);
            }

            if (testResult == null)
                throw new KeyNotFoundException($"Kết quả xét nghiệm với ID {id} không tìm thấy.");

            return MapToResponse(testResult);
        }

        public async Task<TestResultResponseDTO> UpdateTestResult(TestResultUpdateRequestDTO testResult, int id)
        {
            if (testResult == null)
                throw new ArgumentNullException(nameof(testResult));

            if (id <= 0)
                throw new ArgumentException("ID kết quả xét nghiệm không hợp lệ.", nameof(id));

            var existing = await _testResultRepo.GetTestResultById(id);
            if (existing == null)
                throw new KeyNotFoundException($"Kết quả xét nghiệm với ID {id} không tìm thấy.");

            if (testResult.TestDate.HasValue)
                existing.TestDate = testResult.TestDate.Value;
            if (testResult.Result.HasValue)
                existing.ResultValue = testResult.Result.Value;
            if (testResult.Notes != null)
                existing.Notes = testResult.Notes;

            var updated = await _testResultRepo.UpdateTestResult(id, existing);
            if (!updated)
                throw new Exception("Cập nhật thất bại.");

            var updatedResult = await _testResultRepo.GetTestResultById(id);
            return MapToResponse(updatedResult!);
        }

        public async Task<TestResultResponseDTO> CreateTestResultWithComponentTestsAsync(
            TestResultRequestDTO testResult,
            List<ComponentTestResultRequestDTO> componentTestResults,
            int accId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = _context.Database.BeginTransaction())
                {
                    // Validate inputs
                    if (testResult == null)
                        throw new ArgumentNullException(nameof(testResult), "Yêu cầu kết quả xét nghiệm DTO là bắt buộc.");
                    if (componentTestResults == null || !componentTestResults.Any())
                        throw new ArgumentNullException(nameof(componentTestResults), "Cần có ít nhất một kết quả kiểm tra thành phần.");
                    if (testResult.PatientMedicalRecordId <= 0)
                        throw new ArgumentException("ID hồ sơ bệnh án của bệnh nhân không hợp lệ", nameof(testResult.PatientMedicalRecordId));
                    if (accId <= 0)
                        throw new ArgumentException("ID tài khoản không hợp lệ", nameof(accId));

                    await ValidatePatientMedicalRecordExists(testResult.PatientMedicalRecordId);
                    await ValidateAccountExists(accId);

                    // Get staff ID from account ID
                    var staff = await _context.Staff.FirstOrDefaultAsync(s => s.AccId == accId);
                    if (staff == null)
                        throw new InvalidOperationException($"Không tìm thấy nhân viên cho tài khoản với ID {accId}.");

                    // Validate component test results
                    foreach (var component in componentTestResults)
                    {
                        if (string.IsNullOrWhiteSpace(component.ComponentTestResultName))
                            throw new ArgumentException("Tên kết quả kiểm tra thành phần là bắt buộc", nameof(component.ComponentTestResultName));

                        // Validate string length to prevent database errors
                        if (component.ComponentTestResultName.Length > 50) // Adjust based on your database schema
                            throw new ArgumentException("Tên kết quả kiểm tra thành phần quá dài", nameof(component.ComponentTestResultName));

                        if (component.CtrDescription != null && component.CtrDescription.Length > 200)
                            throw new ArgumentException("Mô tả kết quả kiểm tra thành phần quá dài", nameof(component.CtrDescription));

                        if (component.ResultValue != null && component.ResultValue.Length > 20)
                            throw new ArgumentException("Giá trị kết quả kiểm tra thành phần quá dài", nameof(component.ResultValue));

                        if (component.Notes != null && component.Notes.Length > 500)
                            throw new ArgumentException("Ghi chú kết quả kiểm tra thành phần quá dài", nameof(component.Notes));
                    }

                    // Check for duplicate component test result names (case-insensitive)
                    var componentNames = componentTestResults
                        .Select(c => c.ComponentTestResultName.Trim().ToLower())
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .ToList();

                    if (componentNames.Distinct().Count() != componentNames.Count)
                        throw new ArgumentException("Tên kết quả kiểm tra thành phần trùng lặp không được phép trong cùng một kết quả kiểm tra.");

                    try
                    {
                        // Create test result
                        var testResultEntity = MapToRequest(testResult);
                        var createdTestResult = await _testResultRepo.CreateTestResult(testResultEntity);

                        if (createdTestResult == null)
                            throw new InvalidOperationException("Không thể tạo kết quả xét nghiệm.");

                        // Create component test results
                        var componentTestResultEntities = new List<ComponentTestResult>();
                        foreach (var componentRequest in componentTestResults)
                        {
                            var componentEntity = new ComponentTestResult
                            {
                                TrsId = createdTestResult.TrsId,
                                StfId = staff.StfId,
                                CtrName = componentRequest.ComponentTestResultName.Trim(),
                                CtrDescription = componentRequest.CtrDescription?.Trim(),
                                ResultValue = componentRequest.ResultValue?.Trim(),
                                Notes = componentRequest.Notes?.Trim()
                            };

                            var createdComponent = await _componentTestResultRepo.AddTestComponent(componentEntity);
                            if (createdComponent == null)
                                throw new InvalidOperationException($"Không thể tạo thành phần xét nghiệm: {componentEntity.CtrName}");

                            componentTestResultEntities.Add(createdComponent);
                        }

                        // Create notification
                        var notification = new Notification
                        {
                            NotiType = "Kết quả xét nghiệm",
                            NotiMessage = $"Một kết quả xét nghiệm mới với {componentTestResultEntities.Count} thành phần xét nghiệm đã được tạo.",
                            SendAt = DateTime.UtcNow
                        };
                        var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

                        // Get medical record with patient information
                        var medicalRecord = await _context.PatientMedicalRecords
                            .Include(pmr => pmr.Ptn)
                            .FirstOrDefaultAsync(pmr => pmr.PmrId == testResult.PatientMedicalRecordId);

                        if (medicalRecord?.Ptn != null)
                        {
                            // Send notification to patient
                            try
                            {
                                await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, medicalRecord.Ptn.AccId);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Failed to send notification to patient: {ex.Message}");
                                // Don't throw - notification failure shouldn't break the main operation
                            }

                            // Send notification to doctor (if creator is not a doctor)
                            var account = await _context.Accounts.FindAsync(accId);
                            if (account != null && account.Roles != 2) // 2 = Doctor
                            {
                                try
                                {
                                    // Find the most recent appointment for this patient to get the doctor
                                    var recentAppointment = await _context.Appointments
                                        .Include(a => a.Dct)
                                        .Where(a => a.PtnId == medicalRecord.PtnId)
                                        .OrderByDescending(a => a.ApmtDate)
                                        .FirstOrDefaultAsync();

                                    if (recentAppointment?.Dct != null)
                                    {
                                        await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, recentAppointment.Dct.AccId);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"Failed to send notification to doctor: {ex.Message}");
                                    // Don't throw - notification failure shouldn't break the main operation
                                }
                            }
                        }

                        await _context.SaveChangesAsync();
                        transaction.Commit();

                        // Return the created test result with component test results loaded
                        var resultWithComponents = await _context.TestResults
                            .Include(tr => tr.ComponentTestResults)
                            .ThenInclude(ct => ct.Stf)
                            .FirstOrDefaultAsync(tr => tr.TrsId == createdTestResult.TrsId);

                        if (resultWithComponents == null)
                        {
                            Debug.WriteLine("Warning: Could not load created test result with components");
                            return MapToResponse(createdTestResult);
                        }

                        return MapToResponse(resultWithComponents);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to create test result with component tests: {ex.Message}");
                        Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        public async Task<TestResultResponseDTO> UpdateTestResultWithComponentTestsAsync(
            int testResultId,
            TestResultRequestDTO testResult,
            List<ComponentTestResultRequestDTO> componentTestResults,
            int accId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = _context.Database.BeginTransaction();
                try
                {
                    // Validate inputs
                    if (testResult == null)
                        throw new ArgumentNullException(nameof(testResult));
                    if (componentTestResults == null)
                        throw new ArgumentNullException(nameof(componentTestResults));
                    if (testResultId <= 0)
                        throw new ArgumentException("ID kết quả xét nghiệm không hợp lệ", nameof(testResultId));

                    // Validate existence
                    var existingTestResult = await _context.TestResults
                        .Include(tr => tr.ComponentTestResults)
                        .FirstOrDefaultAsync(tr => tr.TrsId == testResultId);

                    if (existingTestResult == null)
                        throw new KeyNotFoundException($"Không tìm thấy kết quả xét nghiệm với ID {testResultId}");

                    await ValidatePatientMedicalRecordExists(testResult.PatientMedicalRecordId);

                    // Update main test result
                    existingTestResult.TestDate = testResult.TestDate;
                    existingTestResult.ResultValue = testResult.Result;
                    existingTestResult.Notes = testResult.Notes;

                    // Handle component test results - OPTIMIZED APPROACH
                    // Get IDs of components that should be updated (those with ID > 0)
                    var updateComponentIds = componentTestResults
                        .Where(c => c.TestResultId > 0)
                        .Select(c => c.TestResultId)
                        .ToHashSet();

                    // Remove components that are no longer in the request
                    var componentsToRemove = existingTestResult.ComponentTestResults
                        .Where(c => !updateComponentIds.Contains(c.CtrId))
                        .ToList();

                    foreach (var component in componentsToRemove)
                    {
                        _context.ComponentTestResults.Remove(component);
                    }

                    // Process each component in the request
                    foreach (var componentRequest in componentTestResults)
                    {
                        if (string.IsNullOrWhiteSpace(componentRequest.ComponentTestResultName))
                            throw new ArgumentException("Tên kết quả kiểm tra thành phần là bắt buộc");

                        if (componentRequest.TestResultId > 0)
                        {
                            // Update existing component
                            var existingComponent = existingTestResult.ComponentTestResults
                                .FirstOrDefault(c => c.CtrId == componentRequest.TestResultId);

                            if (existingComponent != null)
                            {
                                existingComponent.CtrName = componentRequest.ComponentTestResultName.Trim();
                                existingComponent.CtrDescription = componentRequest.CtrDescription?.Trim();
                                existingComponent.ResultValue = componentRequest.ResultValue?.Trim();
                                existingComponent.Notes = componentRequest.Notes?.Trim();
                                existingComponent.StfId = accId;
                            }
                            else
                            {
                                // If for some reason the component with the specified ID doesn't exist, 
                                // treat it as a new component
                                var newComponent = new ComponentTestResult
                                {
                                    TrsId = testResultId,
                                    StfId = accId,
                                    CtrName = componentRequest.ComponentTestResultName.Trim(),
                                    CtrDescription = componentRequest.CtrDescription?.Trim(),
                                    ResultValue = componentRequest.ResultValue?.Trim(),
                                    Notes = componentRequest.Notes?.Trim()
                                };
                                _context.ComponentTestResults.Add(newComponent);
                            }
                        }
                        else
                        {
                            // Add new component (TestResultId is 0 or negative)
                            var newComponent = new ComponentTestResult
                            {
                                TrsId = testResultId,
                                StfId = accId,
                                CtrName = componentRequest.ComponentTestResultName.Trim(),
                                CtrDescription = componentRequest.CtrDescription?.Trim(),
                                ResultValue = componentRequest.ResultValue?.Trim(),
                                Notes = componentRequest.Notes?.Trim()
                            };
                            _context.ComponentTestResults.Add(newComponent);
                        }
                    }

                    // Create notification
                    var notification = new Notification
                    {
                        NotiType = "Cập nhật KQ XN",
                        NotiMessage = $"Kết quả xét nghiệm ID {testResultId} đã được cập nhật.",
                        SendAt = DateTime.UtcNow
                    };
                    var createdNotification = await _notificationRepo.CreateNotificationAsync(notification);

                    // Get medical record with patient information
                    var medicalRecord = await _context.PatientMedicalRecords
                        .Include(pmr => pmr.Ptn)
                        .FirstOrDefaultAsync(pmr => pmr.PmrId == testResult.PatientMedicalRecordId);

                    if (medicalRecord?.Ptn != null)
                    {
                        // Send notification to patient
                        await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, medicalRecord.Ptn.AccId);

                        // Send notification to the most recent doctor for this patient
                        var recentAppointment = await _context.Appointments
                            .Include(a => a.Dct)
                            .Where(a => a.PtnId == medicalRecord.PtnId)
                            .OrderByDescending(a => a.ApmtDate)
                            .FirstOrDefaultAsync();

                        if (recentAppointment?.Dct != null)
                        {
                            await _notificationRepo.SendNotificationToAccIdAsync(createdNotification.NtfId, recentAppointment.Dct.AccId);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Reload the test result with all related data
                    var updatedResult = await _context.TestResults
                        .Include(tr => tr.ComponentTestResults)
                        .ThenInclude(ct => ct.Stf)
                        .FirstAsync(tr => tr.TrsId == testResultId);

                    return MapToResponse(updatedResult);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException($"Cập nhật kết quả xét nghiệm thất bại: {ex.Message}", ex);
                }
            });
        }
    }
}
