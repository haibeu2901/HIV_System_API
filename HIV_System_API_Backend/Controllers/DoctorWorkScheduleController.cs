using HIV_System_API_DTOs.DoctorWorkScheduleDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
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
        public async Task<IActionResult> GetDoctorWorkSchedules()
        {
            var schedules = await _doctorWorkScheduleService.GetDoctorWorkSchedulesAsync();
            if (schedules == null || schedules.Count == 0)
            {
                return NotFound("No doctor work schedules found.");
            }
            return Ok(schedules);
        }

        [HttpGet("GetDoctorWorkScheduleById/{id}")]
        public async Task<IActionResult> GetDoctorWorkScheduleById(int id)
        {
            var schedule = await _doctorWorkScheduleService.GetDoctorWorkScheduleByIdAsync(id);
            if (schedule == null)
            {
                return NotFound($"Doctor work schedule with ID {id} not found.");
            }
            return Ok(schedule);
        }

        [HttpPost("CreateDoctorWorkSchedule")]
        public async Task<IActionResult> CreateDoctorWorkSchedule([FromBody] DoctorWorkScheduleRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                return BadRequest("Request body is null.");
            }

            // Manual model validation for TimeOnly fields
            if (requestDTO.StartTime == default || requestDTO.EndTime == default)
            {
                return BadRequest("StartTime and EndTime must be valid in 'HH:mm:ss' format.");
            }

            try
            {
                var createdSchedule = await _doctorWorkScheduleService.CreateDoctorWorkScheduleAsync(requestDTO);
                if (createdSchedule == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create doctor work schedule.");
                }
                return CreatedAtAction(nameof(GetDoctorWorkScheduleById), new { id = createdSchedule.DwsId }, createdSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("UpdateDoctorWorkSchedule/{id}")]
        public async Task<IActionResult> UpdateDoctorWorkSchedule(int id, [FromBody] DoctorWorkScheduleRequestDTO requestDTO)
        {
            if (requestDTO == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var updatedSchedule = await _doctorWorkScheduleService.UpdateDoctorWorkScheduleAsync(id, requestDTO);
                if (updatedSchedule == null)
                {
                    return NotFound($"Doctor work schedule with ID {id} not found.");
                }
                return Ok(updatedSchedule);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("DeleteDoctorWorkSchedule/{id}")]
        public async Task<IActionResult> DeleteDoctorWorkSchedule(int id)
        {
            try
            {
                var deleted = await _doctorWorkScheduleService.DeleteDoctorWorkScheduleAsync(id);
                if (!deleted)
                {
                    return NotFound($"Doctor work schedule with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

    }
}
