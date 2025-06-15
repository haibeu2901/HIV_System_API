using HIV_System_API_DTOs.PatientDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private IPatientService _patientService;
        private readonly IConfiguration _configuration;

        public PatientController(IConfiguration configuration)
        {
            _patientService = new PatientService();
            _configuration = configuration;
        }

        [HttpGet("GetAllPatients")]
        [Authorize(Roles = "1, 2, 4, 5")]
        public async Task<IActionResult> GetAllPatients()
        {
            try
            {
                var patients = await _patientService.GetAllPatientsAsync();
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetPatientById/{patientId}")]
        [Authorize(Roles = "1, 2, 4, 5")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("CreatePatient")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> CreatePatient([FromBody] PatientRequestDTO patientRequest)
        {
            try
            {
                if (patientRequest == null)
                {
                    return BadRequest("Patient request data is required.");
                }

                var createdPatient = await _patientService.CreatePatientAsync(patientRequest);
                if (createdPatient == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create patient.");
                }

                return CreatedAtAction(nameof(GetPatientById), new { patientId = createdPatient.PtnId }, createdPatient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPut("UpdatePatient/{patientId}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdatePatient(int patientId, [FromBody] PatientRequestDTO patientRequest)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Patient ID must be greater than zero.");
                }
                if (patientRequest == null)
                {
                    return BadRequest("Patient request data is required.");
                }

                var updatedPatient = await _patientService.UpdatePatientAsync(patientId, patientRequest);
                if (updatedPatient == null)
                {
                    return NotFound($"Patient with ID {patientId} not found.");
                }

                return Ok(updatedPatient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpDelete("DeletePatient/{patientId}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeletePatient(int patientId)
        {
            try
            {
                if (patientId <= 0)
                {
                    return BadRequest("Patient ID must be greater than zero.");
                }

                var deleted = await _patientService.DeletePatientAsync(patientId);
                if (!deleted)
                {
                    return NotFound($"Patient with ID {patientId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}
