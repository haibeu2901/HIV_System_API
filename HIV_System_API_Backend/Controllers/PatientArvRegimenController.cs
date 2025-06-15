using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientArvRegimenController : ControllerBase
    {
        private readonly IPatientArvRegimenService _patientArvRegimenService;
        public PatientArvRegimenController()
        {
            _patientArvRegimenService = new PatientArvRegimenService();
        }
        [HttpGet("GetAllPatientArvRegimens")]
        public async Task<IActionResult> GetAllPatientArvRegimens()
        {
            var regimens = await _patientArvRegimenService.GetAllPatientArvRegimensAsync();
            if (regimens == null || regimens.Count == 0)
            {
                return NotFound("No patient ARV regimens found.");
            }
            return Ok(regimens);
        }
        [HttpGet("GetPatientArvRegimenById/{parId}")]
        public async Task<IActionResult> GetPatientArvRegimenById(int parId)
        {
            var regimen = await _patientArvRegimenService.GetPatientArvRegimenByIdAsync(parId);
            if (regimen == null)
            {
                return NotFound($"Patient ARV regimen with ID {parId} not found.");
            }
            return Ok(regimen);
        }
        [HttpPost("CreatePatientArvRegimen")]
        public async Task<IActionResult> CreatePatientArvRegimen([FromBody] PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var createdRegimen = await _patientArvRegimenService.CreatePatientArvRegimenAsync(patientArvRegimen);
                return CreatedAtAction(
                    nameof(GetPatientArvRegimenById), 
                    new { parId = createdRegimen.ParId }, 
                    createdRegimen);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    $"Error creating regimen: {ex.InnerException}");
            }
        }
        [HttpDelete("DeletePatientArvRegimen/{parId}")]
        public async Task<IActionResult> DeletePatientArvRegimen(int parId)
        {
            if (parId <= 0)
            {
                return BadRequest("Invalid regimen ID.");
            }
            var result = await _patientArvRegimenService.DeletePatientArvRegimenAsync(parId);
            if (!result)
            {
                return NotFound($"Patient ARV regimen with ID {parId} not found.");
            }
            return NoContent(); // 204 No Content
        }
        [HttpPut("UpdatePatientArvRegimen/{parId}")]
        public async Task<IActionResult> UpdatePatientArvRegimen(int parId, [FromBody] PatientArvRegimenRequestDTO patientArvRegimen)
        {
            if (patientArvRegimen == null)
            {
                return BadRequest("Request body is null.");
            }
            try
            {
                var updatedRegimen = await _patientArvRegimenService.UpdatePatientArvRegimenAsync(parId, patientArvRegimen);
                if (updatedRegimen == null)
                {
                    return NotFound($"Patient ARV regimen with ID {parId} not found.");
                }
                return Ok(updatedRegimen);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error updating regimen: {ex.InnerException}");
            }

        }
    }
}

