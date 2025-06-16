using HIV_System_API_Backend.Common;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientMedicalRecordController : ControllerBase
    {
        private IPatientMedicalRecordService _patientMedicalRecordService;
        

        public PatientMedicalRecordController(IConfiguration configuration, IMemoryCache memoryCache)
        {
            _patientMedicalRecordService = new PatientMedicalRecordService();
        }

        [HttpGet("GetPatientsMedicalRecord")]
        [Authorize(Roles = "1,2,4,5")]
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
        [Authorize(Roles = "1,2,4,5")]
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
        [Authorize(Roles = "1")]
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
        [Authorize(Roles = "1")]
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
        [Authorize(Roles = "1")]
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
            var accId = ClaimsHelper.ExtractAccountIdFromClaims(User);
            if (accId == null)
            {
                return Unauthorized("Account ID not found in token.");
            }

            var record = await _patientMedicalRecordService.GetPersonalMedicalRecordAsync(accId.Value);
            if (record == null)
            {
                return NotFound("Personal medical record not found.");
            }
            return Ok(record);
        }
    }
}
