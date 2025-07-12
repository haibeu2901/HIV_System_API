using HIV_System_API_Backend.Common;
using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.AppointmentDTO;
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
        [Authorize(Roles = "1, 2, 4, 5")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("CreateAppointment")]
        [Authorize]
        public async Task<ActionResult> CreateAppointment([FromBody] CreateAppointmentRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Appointment data is required.");

            var accountId = ClaimsHelper.ExtractAccountIdFromClaims(User);
            if (!accountId.HasValue)
                return Unauthorized("Invalid user session.");

            try
            {
                // Map AppointmentRequestDTO to CreateAppointmentDTO
                var createDto = new CreateAppointmentRequestDTO
                {
                    DoctorId = dto.DoctorId,
                    AppointmentDate = dto.AppointmentDate,
                    AppointmentTime = dto.AppointmentTime,
                    Notes = dto.Notes
                };

                var createdAppointment = await _appointmentService.CreateAppointmentAsync(createDto, accountId.Value);
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
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpDelete("DeleteAppointment/{id}")]
        [Authorize(Roles = "1")]
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
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetAppointmentById/{id}")]
        [Authorize(Roles = "1, 2, 3, 4, 5")]
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
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPut("UpdateAppointment/{id}")]
        [Authorize(Roles = "1, 2, 4, 5")]
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
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPatch("ChangeAppointmentStatus")]
        [Authorize]
        public async Task<IActionResult> ChangeAppointmentStatusAsync(int id, byte status)
        {
            try
            {
                var updatedAppointment = await _appointmentService.ChangeAppointmentStatusAsync(id, status);
                return Ok(updatedAppointment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }
        
        [HttpPost("UpdateAppointmentRequest")]
        [Authorize]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Appointment data is required.");
            var accountId = ClaimsHelper.ExtractAccountIdFromClaims(User);
            if (!accountId.HasValue)
                return Unauthorized("Invalid user session.");
            try
            {
                var updatedAppointment = await _appointmentService.UpdateAppointmentAsync(id, dto, accountId.Value);
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
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetAllPersonalAppointments")]
        [Authorize]
        public async Task<IActionResult> GetAllPersonalAppointments()
        {
            var accountId = ClaimsHelper.ExtractAccountIdFromClaims(User);
            if (!accountId.HasValue)
                return Unauthorized("Invalid user session.");
            try
            {
                var appointments = await _appointmentService.GetAllPersonalAppointmentsAsync(accountId.Value);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("CompleteAppointment")]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> CompleteAppointment( int appointmentId, [FromBody] CompleteAppointmentDTO dto)
        {
            if (dto == null)
                return BadRequest("Complete appointment data is required.");
            var accountId = ClaimsHelper.ExtractAccountIdFromClaims(User);
            if (!accountId.HasValue)
                return Unauthorized("Invalid user session.");
            try
            {
                var completedAppointment = await _appointmentService.CompleteAppointmentAsync(appointmentId, dto, accountId.Value);
                return Ok(completedAppointment);
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
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}
