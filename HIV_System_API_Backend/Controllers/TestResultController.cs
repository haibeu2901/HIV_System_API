using HIV_System_API_DTOs.TestResultDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    }
}
