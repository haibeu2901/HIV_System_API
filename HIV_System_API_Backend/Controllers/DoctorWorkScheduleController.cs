using HIV_System_API_Backend.Common;
using HIV_System_API_DTOs.DoctorWorkScheduleDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorWorkScheduleController : ControllerBase
    {
        private readonly IDoctorWorkScheduleService _doctorWorkScheduleService;

        public DoctorWorkScheduleController()
        {
            _doctorWorkScheduleService = new DoctorWorkScheduleService();
        }

        [HttpGet("GetDoctorWorkSchedules")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorWorkSchedules()
        {
            try
            {
                var schedules = await _doctorWorkScheduleService.GetDoctorWorkSchedulesAsync();
                if (schedules == null || schedules.Count == 0)
                    return NotFound("No doctor work schedules found.");

                return Ok(schedules);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to retrieve doctor work schedules: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while retrieving doctor work schedules.");
            }
        }

        [HttpGet("GetDoctorWorkScheduleById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorWorkScheduleById(int id)
        {
            try
            {
                var schedule = await _doctorWorkScheduleService.GetDoctorWorkScheduleByIdAsync(id);
                if (schedule == null)
                    return NotFound($"Doctor work schedule with ID {id} not found.");

                return Ok(schedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to retrieve doctor work schedule with ID {id}: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred while retrieving doctor work schedule with ID {id}.");
            }
        }

        [HttpPost("CreateDoctorWorkSchedule")]
        [Authorize (Roles = "1,2,4,5")]
        public async Task<IActionResult> CreateDoctorWorkSchedule([FromBody] DoctorWorkScheduleRequestDTO requestDTO)
        {
            if (requestDTO.StartTime == default || requestDTO.EndTime == default)
                return BadRequest("StartTime and EndTime must be valid in 'HH:mm:ss' format.");

            try
            {
                var createdSchedule = await _doctorWorkScheduleService.CreateDoctorWorkScheduleAsync(requestDTO);
                return CreatedAtAction(nameof(GetDoctorWorkScheduleById), new { id = createdSchedule.DocWorkScheduleId }, createdSchedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to create doctor work schedule: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while creating doctor work schedule.");
            }
        }

        [HttpPut("UpdateDoctorWorkSchedule/{id}")]
        [Authorize (Roles = "1,2,4,5")]
        public async Task<IActionResult> UpdateDoctorWorkSchedule(int id, [FromBody] DoctorWorkScheduleRequestDTO requestDTO)
        {
            if (requestDTO.StartTime == default || requestDTO.EndTime == default)
                return BadRequest("StartTime and EndTime must be valid in 'HH:mm:ss' format.");

            try
            {
                var updatedSchedule = await _doctorWorkScheduleService.UpdateDoctorWorkScheduleAsync(id, requestDTO);
                return Ok(updatedSchedule);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to update doctor work schedule with ID {id}: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred while updating doctor work schedule with ID {id}.");
            }
        }

        [HttpDelete("DeleteDoctorWorkSchedule/{id}")]
        [Authorize (Roles = "1,2,4,5")]
        public async Task<IActionResult> DeleteDoctorWorkSchedule(int id)
        {
            try
            {
                var deleted = await _doctorWorkScheduleService.DeleteDoctorWorkScheduleAsync(id);
                if (!deleted)
                    return NotFound($"Doctor work schedule with ID {id} not found.");

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to delete doctor work schedule with ID {id}: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred while deleting doctor work schedule with ID {id}.");
            }
        }

        [HttpGet("GetPersonalWorkSchedules")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> GetPersonalWorkSchedules()
        {
            int? doctorId = ClaimsHelper.ExtractAccountIdFromClaims(User);

            if (!doctorId.HasValue || doctorId.Value <= 0)
            {
                return BadRequest("Doctor ID must be greater than zero.");
            }

            try
            {
                var schedules = await _doctorWorkScheduleService.GetPersonalWorkSchedulesAsync(doctorId.Value);
                if (schedules == null || schedules.Count == 0)
                    return NotFound($"No work schedules found for doctor with ID {doctorId.Value}.");

                return Ok(schedules);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to retrieve work schedules for doctor with ID {doctorId.Value}: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred while retrieving work schedules for doctor with ID {doctorId.Value}.");
            }
        }

        [HttpGet("GetDoctorWorkSchedulesByDoctorId/{doctorId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDoctorWorkSchedulesByDoctorId(int doctorId)
        {
            try
            {
                var schedules = await _doctorWorkScheduleService.GetDoctorWorkSchedulesByDoctorIdAsync(doctorId);
                if (schedules == null || schedules.Count == 0)
                    return NotFound($"No work schedules found for doctor with ID {doctorId}.");

                return Ok(schedules);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to retrieve work schedules for doctor with ID {doctorId}: {ex.Message}");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An unexpected error occurred while retrieving work schedules for doctor with ID {doctorId}.");
            }
        }
    }
}
