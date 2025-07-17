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

        public TestResultController()
        {
            _testResultService = new TestResultService();
        }

        [HttpPost ("Create")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> CreateTestResult([FromBody] TestResultRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Test result data is required.");

            var result = await _testResultService.CreateTestResult(dto);
            return CreatedAtAction(nameof(GetTestResultById), new { id = result.TestResultId }, result);
        }

        [HttpGet("GetById/{id:int}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> GetTestResultById(int id)
        {
            try
            {
                var result = await _testResultService.GetTestResultById(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<TestResultResponseDTO>>> GetAllTestResult()
        {
            var results = await _testResultService.GetAllTestResult();
            return Ok(results);
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> DeleteTestResult(int id)
        {
            var deleted = await _testResultService.DeleteTestResult(id);
            if (!deleted)
                return NotFound($"Test result with ID {id} not found.");
            return NoContent();
        }

        [HttpPut("UpdateTestResult/{id:int}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> UpdateTestResult(int id, [FromBody] TestResultUpdateRequestDTO dto)
        {
            try
            {
                var result = await _testResultService.UpdateTestResult(dto, id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("personal")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> GetPersonalTestResult()
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
                return NotFound(ex.Message);
            }
        }

        [HttpGet("Sustain")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<TestResultResponseDTO>>> GetSustainTestResultPatient()
        {
            var results = await _testResultService.GetSustainTestResultPatient();
            return Ok(results);
        }

        /// <summary>
        /// Creates a new test result with associated component tests.
        /// </summary>
        /// <param name="request">Object containing test result and component test data.</param>
        /// <returns>Created test result with component tests.</returns>
        [HttpPost("CreateTestResultWithComponentTests")]
        [Authorize(Roles = "1,4,5")]
        public async Task<IActionResult> CreateTestResultWithComponentTests([FromBody] CreatePatientTestResultWithComponentTestsRequestDTO request)
        {
            // Validate request structure
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

            // Validate test result data
            if (request.TestResult.PatientMedicalRecordId <= 0)
            {
                return BadRequest("Patient medical record ID must be greater than 0.");
            }

            // Validate component test data
            foreach (var componentTest in request.ComponentTests)
            {
                if (string.IsNullOrWhiteSpace(componentTest.ComponentTestResultName))
                {
                    return BadRequest("All component tests must have a valid name.");
                }
            }

            // Check for duplicate component test names
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
                // Log the full exception details for debugging (uncomment when logger is available)
                // _logger.LogError(ex, "Database error while creating test result with component tests");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Database error occurred while creating test result. Please try again.");
            }
            catch (Exception ex)
            {
                // Log the full exception details for debugging (uncomment when logger is available)
                // _logger.LogError(ex, "Unexpected error creating test result with component tests");

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred. Please try again.");
            }
        }
    }
}
