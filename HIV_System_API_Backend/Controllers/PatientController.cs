using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private IPatientService _patientService;

        public PatientController()
        {
            _patientService = new PatientService();
        }

        [HttpGet("GetAllPatients")]
        public async Task<IActionResult> GetAllPatients()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetPatientById/{patientId}")]
        public async Task<IActionResult> GetPatientById(int patientId)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Patient ID must be greater than zero.");
                }
                var patient = await _patientService.GetPatientByIdAsync(patientId);
                if (patient == null)
                {
                    return NotFound($"Patient with ID {patientId} not found.");
                }
                return Ok(patient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreatePatient")]
        public async Task<IActionResult> CreatePatient(int accId)
        {
            try
            {
                if (accId <= 0)
                {
                    return BadRequest("Account ID must be greater than zero.");
                }

                var createdPatient = await _patientService.CreatePatientAsync(accId);
                if (createdPatient == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create patient.");
                }

                return CreatedAtAction(nameof(GetPatientById), new { patientId = createdPatient.PtnId }, createdPatient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdatePatient/{patientId}")]
        public async Task<IActionResult> UpdatePatient(int patientId, [FromBody] PatientDTO patientDTO)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Patient ID must be greater than zero.");
                }
                if (patientDTO == null)
                {
                    return BadRequest("Patient data is required.");
                }

                var existingPatient = await _patientService.GetPatientByIdAsync(patientId);
                if (existingPatient == null)
                {
                    return NotFound($"Patient with ID {patientId} not found.");
                }

                var updateResult = await _patientService.UpdatePatientAsync(patientId, patientDTO);
                if (!updateResult)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update patient.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeletePatient/{patientId}")]
        public async Task<IActionResult> DeletePatient(int patientId)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Patient ID must be greater than zero.");
                }

                var existingPatient = await _patientService.GetPatientByIdAsync(patientId);
                if (existingPatient == null)
                {
                    return NotFound($"Patient with ID {patientId} not found.");
                }

                var deleteResult = await _patientService.DeletePatientAsync(patientId);
                if (!deleteResult)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete patient.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
