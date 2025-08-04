using HIV_System_API_BOs;
using HIV_System_API_DTOs.ComponentTestResultDTO;
using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace HIV_System_API_Services.Implements
{
    public class TestResultService : ITestResultService
    {
        private readonly ITestResultRepo _testResultRepo;
        private readonly IComponentTestResultRepo _componentTestResultRepo;
        private readonly IComponentTestResultService _componentTestResultService;
        private readonly IPatientRepo _patientRepo;
        private readonly INotificationRepo _notificationRepo;
        private readonly HivSystemApiContext _context;

        public TestResultService()
        {
            _testResultRepo = new TestResultRepo() ?? throw new ArgumentNullException(nameof(_testResultRepo));
            _componentTestResultRepo = new ComponentTestResultRepo() ?? throw new ArgumentNullException(nameof(_componentTestResultRepo));
            _componentTestResultService = new ComponentTestResultService() ?? throw new ArgumentNullException(nameof(_componentTestResultService));
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

        private async Task<TestResultResponseDTO> MapToResponseAsync(TestResult testResult)
        {
            var refreshedResult = await _context.TestResults
                .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.TrsId == testResult.TrsId);

            if (refreshedResult == null)
                throw new KeyNotFoundException($"Test result with ID {testResult.TrsId} not found");

            return new TestResultResponseDTO
            {
                PatientMedicalRecordId = refreshedResult.PmrId,
                TestDate = refreshedResult.TestDate,
                Result = refreshedResult.ResultValue,
                Notes = refreshedResult.Notes,
                TestResultId = refreshedResult.TrsId,
                ComponentTestResults = refreshedResult.ComponentTestResults?
                    .OrderBy(ct => ct.CtrName)
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
                throw new InvalidOperationException($"Hồ sơ bệnh án với ID {pmrId} không tồn tại.");
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
            return await MapToResponseAsync(created);
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
            var results = await _testResultRepo.GetAllTestResult();

            if (results.Any(r => r.ComponentTestResults == null || !r.ComponentTestResults.Any()))
            {
                results = await _context.TestResults
                    .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                    .OrderByDescending(tr => tr.TestDate)
                    .ThenByDescending(tr => tr.TrsId)
                    .ToListAsync();
            }
            else
            {
                results = results.OrderByDescending(r => r.TestDate).ToList();
            }

            var responseList = new List<TestResultResponseDTO>();
            foreach (var result in results)
            {
                responseList.Add(await MapToResponseAsync(result));
            }
            return responseList;
        }

        public async Task<List<PersonalTestResultResponseDTO>> GetPersonalTestResult(int id)
        {
            var patient = await _patientRepo.GetPatientByIdAsync(id);
            if (patient == null)
                throw new KeyNotFoundException($"Bệnh nhân với ID {id} không tìm thấy.");

            var results = await _testResultRepo.GetTestResultsByPatientId(patient.PatientMedicalRecord.PmrId);
            if (results == null || !results.Any())
                throw new KeyNotFoundException($"Không tìm thấy kết quả xét nghiệm cho bệnh nhân với ID {id}.");

            var orderedResults = results.OrderByDescending(r => r.TestDate).ThenByDescending(r => r.TrsId);

            var response = new List<PersonalTestResultResponseDTO>();
            foreach (var r in orderedResults)
            {
                var refreshedResult = await _context.TestResults
                    .Include(tr => tr.ComponentTestResults)
                    .ThenInclude(ct => ct.Stf)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(tr => tr.TrsId == r.TrsId);

                response.Add(new PersonalTestResultResponseDTO
                {
                    PatientMedicalRecordId = refreshedResult.PmrId,
                    TestDate = refreshedResult.TestDate,
                    Result = refreshedResult.ResultValue,
                    Notes = refreshedResult.Notes,
                    ComponentTestResults = refreshedResult.ComponentTestResults?
                        .OrderBy(ct => ct.CtrName)
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
                });
            }
            return response;
        }

        public async Task<List<TestResultResponseDTO>> GetSustainTestResultPatient()
        {
            var positiveResults = await _context.TestResults
                .Include(tr => tr.ComponentTestResults)
                .ThenInclude(ct => ct.Stf)
                .Where(r => r.ResultValue == true)
                .OrderByDescending(r => r.TestDate)
                .ThenByDescending(r => r.TrsId)
                .ToListAsync();

            var responseList = new List<TestResultResponseDTO>();
            foreach (var result in positiveResults)
            {
                responseList.Add(await MapToResponseAsync(result));
            }
            return responseList;
        }

        public async Task<TestResultResponseDTO> GetTestResultById(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID kết quả xét nghiệm không hợp lệ", nameof(id));

            var testResult = await _testResultRepo.GetTestResultById(id);
            if (testResult == null)
                throw new KeyNotFoundException($"Kết quả xét nghiệm với ID {id} không tìm thấy.");

            return await MapToResponseAsync(testResult);
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
            return await MapToResponseAsync(updatedResult!);
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

                    // Validate date and time - the testate must be today
                    var currentDate = DateOnly.FromDateTime(DateTime.Now);
                    if (testResult.TestDate > currentDate)
                        throw new ArgumentException("Ngày xét nghiệm không thể ở tương lai", nameof(testResult.TestDate));
                    if (testResult.TestDate < currentDate)
                        throw new ArgumentException("Ngày xét nghiệm quá xa trong quá khứ", nameof(testResult.TestDate));

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
                            SendAt = DateTime.Now
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
                            return await MapToResponseAsync(createdTestResult);
                        }

                        return await MapToResponseAsync(resultWithComponents);
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

                    if (existingTestResult.TestDate < DateOnly.FromDateTime(DateTime.Now))
                        throw new InvalidOperationException("Không thể cập nhật kết quả xét nghiệm ở trong quá khứ.");

                    // Update main test result
                    existingTestResult.TestDate = testResult.TestDate;
                    existingTestResult.ResultValue = testResult.Result;
                    existingTestResult.Notes = testResult.Notes;

                    // Handle component test results - AUTO MATCH BY COMPONENT NAME
                    // Get existing components and create dictionary for easier lookup
                    var existingComponents = existingTestResult.ComponentTestResults.ToList();
                    var existingComponentDict = existingComponents.ToDictionary(c => c.CtrName.Trim().ToLower(), c => c);
                    var requestComponentDict = componentTestResults.ToDictionary(c => c.ComponentTestResultName.Trim().ToLower(), c => c);

                    var updatedComponents = new List<ComponentTestResult>();

                    // Process each component in the request
                    foreach (var componentRequest in componentTestResults)
                    {
                        if (string.IsNullOrWhiteSpace(componentRequest.ComponentTestResultName))
                            throw new ArgumentException("Tên kết quả kiểm tra thành phần là bắt buộc");

                        var componentNameKey = componentRequest.ComponentTestResultName.Trim().ToLower();

                        if (existingComponentDict.ContainsKey(componentNameKey))
                        {
                            // Update existing component - match by component name
                            var existingComponent = existingComponentDict[componentNameKey];
                            var updatedComponent = new ComponentTestResult
                            {
                                CtrId = existingComponent.CtrId,
                                TrsId = existingComponent.TrsId,
                                StfId = accId, // Update with current staff ID
                                CtrName = componentRequest.ComponentTestResultName?.Trim(),
                                CtrDescription = componentRequest.CtrDescription?.Trim(),
                                ResultValue = componentRequest.ResultValue?.Trim(),
                                Notes = componentRequest.Notes?.Trim()
                            };

                            await _componentTestResultRepo.UpdateTestComponent(updatedComponent);
                            updatedComponents.Add(updatedComponent);
                        }
                        else
                        {
                            // Add new component - component name doesn't exist
                            var newComponent = new ComponentTestResult
                            {
                                TrsId = testResultId,
                                StfId = accId,
                                CtrName = componentRequest.ComponentTestResultName?.Trim(),
                                CtrDescription = componentRequest.CtrDescription?.Trim(),
                                ResultValue = componentRequest.ResultValue?.Trim(),
                                Notes = componentRequest.Notes?.Trim()
                            };

                            var createdComponent = await _componentTestResultRepo.AddTestComponent(newComponent);
                            updatedComponents.Add(createdComponent);
                        }
                    }

                    // Delete components that are no longer in the request
                    var componentsToDelete = existingComponents.Where(ec => !requestComponentDict.ContainsKey(ec.CtrName.Trim().ToLower())).ToList();
                    foreach (var componentToDelete in componentsToDelete)
                    {
                        await _componentTestResultRepo.DeleteTestComponent(componentToDelete.CtrId);
                    }

                    // Create notification
                    var notification = new Notification
                    {
                        NotiType = "Cập nhật KQ XN",
                        NotiMessage = $"Kết quả xét nghiệm ID {testResultId} đã được cập nhật với {updatedComponents.Count} thành phần.",
                        SendAt = DateTime.Now
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

                    return await MapToResponseAsync(updatedResult);
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
