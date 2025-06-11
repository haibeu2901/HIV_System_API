using HIV_System_API_BOs;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentController : ControllerBase
    {
        private IAppointmentService _appointmentService;

        public AppointmentController()
        {
            _appointmentService = new AppointmentService();
        }

        [HttpGet("GetAllAppointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (appointment == null)
            {
                return BadRequest("Appointment cannot be null");
            }
            try
            {
                var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
                return CreatedAtAction(nameof(GetAllAppointments), new { id = createdAppointment.ApmId }, createdAppointment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteAppointment/{id}")]
        public async Task<IActionResult> DeleteAppointmentByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid appointment ID.");
            }

            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound($"Appointment with ID {id} not found.");
                }

                var result = await _appointmentService.DeleteAppointmentByIdAsync(id);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete the appointment.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAppointmentById/{id}")]
        public async Task<IActionResult> GetAppointmentByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid appointment ID.");
            }
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    return NotFound($"Appointment with ID {id} not found.");
                }
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateAppointment/{id}")]
        public async Task<IActionResult> UpdateAppointmentByIdAsync(int id, [FromBody] Appointment appointment)
        {
            if (id <= 0 || appointment == null)
            {
                return BadRequest("Invalid appointment ID or appointment data.");
            }
            try
            {
                var existingAppointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (existingAppointment == null)
                {
                    return NotFound($"Appointment with ID {id} not found.");
                }

                var result = await _appointmentService.UpdateAppointmentByIdAsync(id);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the appointment.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("ChangeAppointmentStatus/{id}/{status}")]
        public async Task<IActionResult> ChangeAppointmentStatusAsync(int id, byte status)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid appointment ID.");
            }
            try
            {
                var result = await _appointmentService.ChangeAppointmentStatusAsync(id, status);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to change the appointment status.");
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
