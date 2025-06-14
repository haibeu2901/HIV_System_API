using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientMedicalRecordController : ControllerBase
    {
        private IPatientMedicalRecordService _patientMedicalRecordService;
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;

        public PatientMedicalRecordController(IConfiguration configuration)
        {
            _patientMedicalRecordService = new PatientMedicalRecordService();
            _configuration = configuration;
            _accountService = new AccountService();
        }

        [HttpGet("GetPatientsMedicalRecord")]
        public async Task<IActionResult> GetAllPatientMedicalRecords()
        {
            var records = await _patientMedicalRecordService.GetAllPatientMedicalRecordsAsync();
            if (records == null || records.Count == 0)
            {
                return NotFound("No patient medical records found.");
            }
            return Ok(records);
        }

        [HttpGet("GetPatientMedicalRecordById/{id}")]
        public async Task<IActionResult> GetPatientMedicalRecordById(int id)
        {
            var record = await _patientMedicalRecordService.GetPatientMedicalRecordByIdAsync(id);
            if (record == null)
            {
                return NotFound($"Patient medical record with ID {id} not found.");
            }
            return Ok(record);
        }

        [HttpPost("CreatePatientMedicalRecord")]
        public async Task<IActionResult> CreatePatientMedicalRecord([FromBody] PatientMedicalRecordRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var createdRecord = await _patientMedicalRecordService.CreatePatientMedicalRecordAsync(requestDTO);
                if (createdRecord == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create patient medical record.");
                }
                return CreatedAtAction(nameof(GetPatientMedicalRecordById), new { id = createdRecord.PmrId }, createdRecord);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("UpdatePatientMedicalRecord/{id}")]
        public async Task<IActionResult> UpdatePatientMedicalRecord(int id, [FromBody] PatientMedicalRecordRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var updatedRecord = await _patientMedicalRecordService.UpdatePatientMedicalRecordAsync(id, requestDTO);
                if (updatedRecord == null)
                {
                    return NotFound($"Patient medical record with ID {id} not found.");
                }
                return Ok(updatedRecord);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("DeletePatientMedicalRecord/{id}")]
        public async Task<IActionResult> DeletePatientMedicalRecord(int id)
        {
            try
            {
                var deleted = await _patientMedicalRecordService.DeletePatientMedicalRecordAsync(id);
                if (!deleted)
                {
                    return NotFound($"Patient medical record with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("GetPersonalMedicalRecord")]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> GetPersonalMedicalRecord()
        {
            var account = ExtractAccountInfoFromClaims(User);

            if (string.IsNullOrEmpty(account.AccUsername) || string.IsNullOrEmpty(account.AccPassword))
            {
                return Unauthorized("User identity or password not found in token.");
            }

            if (account == null)
            {
                return NotFound("Account not found.");
            }

            var record = await _patientMedicalRecordService.GetPersonalMedicalRecordAsync(account.AccId);
            if (record == null)
                return NotFound("Personal medical record not found.");

            return Ok(record);
        }
        // Helper method to extract account info from JWT claims
        private AccountResponseDTO? ExtractAccountInfoFromClaims(ClaimsPrincipal user)
        {
            var username = user?.Claims.FirstOrDefault(c => c.Type == "AccUsername")?.Value;
            var password = user?.Claims.FirstOrDefault(c => c.Type == "AccPassword")?.Value;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            // Assuming _accountService is available and synchronous call is acceptable for this helper
            // If not, consider making this method async and updating all usages accordingly
            var accountTask = _accountService.GetAccountByLoginAsync(username, password);
            accountTask.Wait();
            return accountTask.Result;
        }
    }
}
