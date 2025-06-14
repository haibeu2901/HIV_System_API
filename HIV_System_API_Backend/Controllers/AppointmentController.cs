using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
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
    public class AppointmentController : ControllerBase
    {
        private IAppointmentService _appointmentService;

        public AppointmentController()
        {
            _appointmentService = new AppointmentService();
        }

        [HttpGet("GetAllAppointments")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateAppointment")]
        [Authorize]
        public async Task<ActionResult> CreateAppointment([FromBody] AppointmentRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Appointment data is required.");

            try
            {
                var createdAppointment = await _appointmentService.CreateAppointmentAsync(dto);
                return CreatedAtAction(nameof(GetAppointmentById), new { id = createdAppointment.AppointmentId }, createdAppointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteAppointment/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAppointmentByIdAsync(int id)
        {
            try
            {
                var deleted = await _appointmentService.DeleteAppointmentByIdAsync(id);
                if (!deleted)
                    return NotFound($"Appointment with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAppointmentById/{id}")]
        [Authorize(Roles = "1,2")]
        public async Task<ActionResult> GetAppointmentById(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
                if (appointment == null)
                    return NotFound($"Appointment with ID {id} not found.");

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateAppointment/{id}")]
        public async Task<IActionResult> UpdateAppointmentByIdAsync(int id, [FromBody] AppointmentRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Appointment data is required.");

            try
            {
                var updatedAppointment = await _appointmentService.UpdateAppointmentByIdAsync(id, dto);
                return Ok(updatedAppointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("ChangeAppointmentStatus/{id}/{status}")]
        public async Task<IActionResult> ChangeAppointmentStatusAsync(int id, byte status)
        {
            try
            {
                var updatedAppointment = await _appointmentService.ChangeAppointmentStatusAsync(id, status);
                return Ok(updatedAppointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("my-appointments")]
        [Authorize(Roles = "2,3")] // Only doctors and patients
        public async Task<ActionResult<List<AppointmentResponseDTO>>> GetMyAppointments()
        {
            try
            {
                // Get current user's role and ID from the token
                var currentUserRole = byte.Parse(User.FindFirst(ClaimTypes.Role)?.Value ?? "0");
                var currentUserId = int.Parse(User.FindFirst("AccountId")?.Value ?? "0");

                if (currentUserId == 0)
                {
                    return Unauthorized("Invalid user session.");
                }

                var appointments = await _appointmentService.GetAppointmentsByAccountIdAsync(currentUserId, currentUserRole);
                return Ok(appointments);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
