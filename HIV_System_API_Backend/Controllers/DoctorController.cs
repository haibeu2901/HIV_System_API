using HIV_System_API_DTOs.DoctorDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IConfiguration _configuration;

        public DoctorController(IConfiguration configuration)
        {
            _doctorService = new DoctorService();
            _configuration = configuration;
        }

        [HttpGet("GetAllDoctors")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _doctorService.GetAllDoctorsAsync();
            if (doctors == null || doctors.Count == 0)
            {
                return NotFound("No doctors found.");
            }
            return Ok(doctors);
        }

        [HttpGet("GetDoctorById")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _doctorService.GetDoctorByIdAsync(id);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID {id} not found.");
            }
            return Ok(doctor);
        }

        [HttpPost("CreateDoctor")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorRequestDTO doctorRequestDTO)
        {
            if (doctorRequestDTO == null)
            {
                return BadRequest("Doctor data is required.");
            }

            try
            {
                var createdDoctor = await _doctorService.CreateDoctorAsync(doctorRequestDTO);
                if (createdDoctor == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create doctor.");
                }
                return CreatedAtAction(nameof(GetDoctorById), new { id = createdDoctor.DoctorId }, createdDoctor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }

        [HttpPut("UpdateDoctor")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorRequestDTO doctorRequest)
        {
            if (doctorRequest == null)
            {
                return BadRequest("Doctor data is required.");
            }

            try
            {
                var updatedDoctor = await _doctorService.UpdateDoctorAsync(id, doctorRequest);
                if (updatedDoctor == null)
                {
                    return NotFound($"Doctor with ID {id} not found.");
                }
                return Ok(updatedDoctor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }

        [HttpDelete("DeleteDoctor")]
        [Authorize (Roles = "1,5")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            try
            {
                var deleted = await _doctorService.DeleteDoctorAsync(id);
                if (!deleted)
                {
                    return NotFound($"Doctor with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }

        [HttpGet("GetDoctorByDateAndTime")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> GetDoctorByDateAndTime(DateOnly Date, TimeOnly Time)
        {
            try
            {
                var doctors = await _doctorService.GetDoctorsByDateAndTimeAsync(Date, Time);
                if (doctors == null || doctors.Count == 0)
                {
                    return NotFound("No doctors available at the specified date and time.");
                }
                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.InnerException}");
            }
        }
    }
}
