using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestResultController : ControllerBase
    {
        private readonly ITestResultService _testResultService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<TestResultController> _logger;

        public TestResultController(ITestResultService testResultService, IMemoryCache cache, ILogger<TestResultController> logger)
        {
            _testResultService = testResultService ?? throw new ArgumentNullException(nameof(testResultService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
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
                
                // Clear related cache entries
                _cache.Remove($"test_results_all");
                _cache.Remove($"sustain_test_results");
                
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

        [HttpGet("GetById/{id:int}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<TestResultResponseDTO>> GetTestResultById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid test result ID.");

            try
            {
                var cacheKey = $"test_result_{id}";
                if (_cache.TryGetValue(cacheKey, out TestResultResponseDTO? cachedResult) && cachedResult != null)
                {
                    return Ok(cachedResult);
                }

                var result = await _testResultService.GetTestResultById(id);
                
                // Cache for 5 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning("Test result not found: {Id}", id);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test result {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the test result.");
            }
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<TestResultResponseDTO>>> GetAllTestResult()
        {
            try
            {
                var cacheKey = "test_results_all";
                if (_cache.TryGetValue(cacheKey, out List<TestResultResponseDTO>? cachedResults) && cachedResults != null)
                {
                    return Ok(cachedResults);
                }

                var results = await _testResultService.GetAllTestResult();
                
                // Cache for 2 minutes since this data changes frequently
                _cache.Set(cacheKey, results, TimeSpan.FromMinutes(2));
                
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
                
                // Clear related cache entries
                _cache.Remove($"test_result_{id}");
                _cache.Remove($"test_results_all");
                _cache.Remove($"sustain_test_results");
                
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
                
                // Clear related cache entries
                _cache.Remove($"test_result_{id}");
                _cache.Remove($"test_results_all");
                _cache.Remove($"sustain_test_results");
                
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
                var cacheKey = $"personal_test_results_{accId}";
                if (_cache.TryGetValue(cacheKey, out List<PersonalTestResultResponseDTO>? cachedResults) && cachedResults != null)
                {
                    return Ok(cachedResults);
                }

                var result = await _testResultService.GetPersonalTestResult(accId);
                
                // Cache for 5 minutes
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                
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
                var cacheKey = "sustain_test_results";
                if (_cache.TryGetValue(cacheKey, out List<TestResultResponseDTO>? cachedResults) && cachedResults != null)
                {
                    return Ok(cachedResults);
                }

                var results = await _testResultService.GetSustainTestResultPatient();
                
                // Cache for 10 minutes since this data doesn't change as frequently
                _cache.Set(cacheKey, results, TimeSpan.FromMinutes(10));
                
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sustain test results");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving sustain test results.");
            }
        }
    }
}
