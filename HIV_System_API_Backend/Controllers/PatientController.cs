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

        [HttpGet("GetPatientsByName/{name}")]
        public async Task<IActionResult> GetPatientsByName(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Name cannot be null or empty.");
                }
                var patients = await _patientService.GetPatientsByNameAsync(name);
                if (patients == null || !patients.Any())
                {
                    return NotFound($"No patients found with name containing '{name}'.");
                }
                return Ok(patients);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
