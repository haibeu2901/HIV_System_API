using HIV_System_API_Backend.Common;
using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultController : ControllerBase
    {
        private readonly ITestResultService _testResultService;
        private readonly ILogger<TestResultController> _logger;

        public TestResultController(ITestResultService testResultService, ILogger<TestResultController> logger)
        {
            _testResultService = testResultService ?? throw new ArgumentNullException(nameof(testResultService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("Create")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> CreateTestResult([FromBody] TestResultRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Test result data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _testResultService.CreateTestResult(dto);
                return CreatedAtAction(nameof(GetTestResultById), new { id = result.TestResultId }, result);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid test result creation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test result");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the test result.");
            }
        }

        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> GetTestResultById(int id)
        {
            _logger.LogInformation("Attempting to retrieve test result with ID: {Id}", id);

            if (id <= 0)
            {
                _logger.LogWarning("Invalid test result ID provided: {Id}", id);
                return BadRequest("Invalid test result ID.");
            }

            try
            {
                var result = await _testResultService.GetTestResultById(id);

                if (result == null)
                {
                    _logger.LogWarning("Test result not found for ID: {Id}", id);
                    return NotFound($"Test result with ID {id} not found.");
                }

                _logger.LogInformation("Successfully retrieved test result ID: {Id}", id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Test result not found: {Id}", id);
                return NotFound($"Test result with ID {id} not found: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid argument for test result retrieval: {Id}", id);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error retrieving test result {Id}", id);
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving the test result."
                );
            }
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<TestResultResponseDTO>>> GetAllTestResult()
        {
            try
            {
                var results = await _testResultService.GetAllTestResult();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all test results");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving test results.");
            }
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> DeleteTestResult(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid test result ID.");

            try
            {
                var deleted = await _testResultService.DeleteTestResult(id);
                if (!deleted)
                    return NotFound($"Test result with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting test result {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the test result.");
            }
        }

        [HttpPut("UpdateTestResult/{id:int}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> UpdateTestResult(int id, [FromBody] TestResultUpdateRequestDTO dto)
        {
            if (id <= 0)
                return BadRequest("Invalid test result ID.");

            if (dto == null)
                return BadRequest("Test result data is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _testResultService.UpdateTestResult(dto, id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Test result not found for update: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("Invalid test result update request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating test result {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the test result.");
            }
        }

        [HttpGet("personal")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<List<PersonalTestResultResponseDTO>>> GetPersonalTestResult()
        {
            var accIdClaim = User.FindFirst("AccountId")?.Value;
            if (string.IsNullOrEmpty(accIdClaim) || !int.TryParse(accIdClaim, out int accId) || accId <= 0)
                return Unauthorized("Invalid user session.");

            try
            {
                var result = await _testResultService.GetPersonalTestResult(accId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Personal test results not found for account {AccId}", accId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving personal test results for account {AccId}", accId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving personal test results.");
            }
        }

        [HttpGet("Sustain")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<TestResultResponseDTO>>> GetSustainTestResultPatient()
        {
            try
            {
                var results = await _testResultService.GetSustainTestResultPatient();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sustain test results");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving sustain test results.");
            }
        }

        [HttpPost("CreateTestResultWithComponentTests")]
        [Authorize(Roles = "1,4,5")]
        public async Task<IActionResult> CreateTestResultWithComponentTests([FromBody] CreatePatientTestResultWithComponentTestsRequestDTO request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }

            if (request.TestResult == null)
            {
                return BadRequest("Test result data is required.");
            }

            if (request.ComponentTests == null || !request.ComponentTests.Any())
            {
                return BadRequest("At least one component test is required.");
            }

            if (request.TestResult.PatientMedicalRecordId <= 0)
            {
                return BadRequest("Patient medical record ID must be greater than 0.");
            }

            foreach (var componentTest in request.ComponentTests)
            {
                if (string.IsNullOrWhiteSpace(componentTest.ComponentTestResultName))
                {
                    return BadRequest("All component tests must have a valid name.");
                }
            }

            var componentTestNames = request.ComponentTests
                .Select(ct => ct.ComponentTestResultName?.Trim().ToLower())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (componentTestNames.Distinct().Count() != componentTestNames.Count)
            {
                return BadRequest("Component test names must be unique within the same test result.");
            }

            try
            {
                var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (accId == null)
                {
                    return Unauthorized("Account ID not found in token.");
                }

                var createdTestResult = await _testResultService.CreateTestResultWithComponentTestsAsync(
                    request.TestResult,
                    request.ComponentTests,
                    accId.Value);

                return CreatedAtAction(
                    nameof(GetTestResultById),
                    new { id = createdTestResult.TestResultId },
                    createdTestResult);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Resource not found: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Database error occurred while creating test result. Please try again.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again.");
            }
        }

        [HttpPut("UpdateTestResultWithComponentTests/{testResultId}")]
        [Authorize(Roles = "1,4,5")]
        public async Task<IActionResult> UpdateTestResultWithComponentTests(
            int testResultId,
            [FromBody] UpdatePatientTestResultWithComponentTestsRequestDTO request)
        {
            if (testResultId <= 0)
            {
                return BadRequest("Test result ID must be greater than 0.");
            }

            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }
            if (request.TestResult == null)
            {
                return BadRequest("Test result data is required.");
            }
            if (request.ComponentTests == null || !request.ComponentTests.Any())
            {
                return BadRequest("At least one component test is required.");
            }

            if (request.TestResult.PatientMedicalRecordId <= 0)
            {
                return BadRequest("Patient medical record ID must be greater than 0.");
            }

            foreach (var componentTest in request.ComponentTests)
            {
                if (string.IsNullOrWhiteSpace(componentTest.ComponentTestResultName))
                {
                    return BadRequest("All component tests must have a valid name.");
                }
            }

            var componentTestNames = request.ComponentTests
                .Select(ct => ct.ComponentTestResultName?.Trim().ToLower())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();
            if (componentTestNames.Distinct().Count() != componentTestNames.Count)
            {
                return BadRequest("Component test names must be unique within the same test result.");
            }

            try
            {
                var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);
                if (accId == null)
                {
                    return Unauthorized("Account ID not found in token.");
                }

                var updatedTestResult = await _testResultService.UpdateTestResultWithComponentTestsAsync(
                    testResultId,
                    request.TestResult,
                    request.ComponentTests,
                    accId.Value);

                return Ok(updatedTestResult);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound($"Resource not found: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Database error occurred while updating test result. Please try again.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again.");
            }
        }
    }
}
