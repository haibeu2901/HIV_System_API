using HIV_System_API_BOs;
using HIV_System_API_DTOs.Appointment;
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
        public async Task<ActionResult<AppointmentDTO>> CreateAppointment([FromBody] AppointmentCreateDTO dto)
        {
            if(dto == null)
            {
                return BadRequest("Appointment data is required.");
            }
            try
            {
                var appointment = new Appointment
                {
                    PmrId = dto.PmrId,
                    DctId = dto.DctId,
                    ApmtDate = dto.ApmtDate,
                    ApmTime = dto.ApmTime,
                    ApmStatus = dto.ApmStatus,
                    Notes = dto.Notes,
                    Dct = null,
                    Pmr = null
                };
                
                var createdAppointment = await _appointmentService.CreateAppointmentAsync(appointment);
                var result = await _appointmentService.GetAppointmentByIdAsync(createdAppointment.ApmId);
                
                return CreatedAtAction(
                    actionName: "GetAppointmentById",
                    routeValues: new { id = createdAppointment.ApmId },
                    value: result
                );
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
        public async Task<ActionResult<AppointmentDTO>> GetAppointmentById(int id)
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
        public async Task<IActionResult> UpdateAppointmentByIdAsync(int id, [FromBody] AppointmentUpdateDTO dto)
        {
            if (id <= 0 || dto == null)
            {
                return BadRequest("Invalid appointment ID or data.");
            }
            try
            {
                // Create Appointment object with updated values
                var appointment = new Appointment
                {
                    ApmId = id,  // Important: Include the ID
                    PmrId = dto.PmrId,
                    DctId = dto.DctId,
                    ApmtDate = dto.ApmtDate,
                    ApmTime = dto.ApmTime,
                    ApmStatus = dto.ApmStatus,
                    Notes = dto.Notes
                };

                var result = await _appointmentService.UpdateAppointmentByIdAsync(appointment);
                if (!result)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update the appointment.");
                }
                
                // Get updated appointment to return
                var updatedAppointment = await _appointmentService.GetAppointmentByIdAsync(id);
                return Ok(updatedAppointment);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPatch("ChangeAppointmentStatus/{id}/{status}")]
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
