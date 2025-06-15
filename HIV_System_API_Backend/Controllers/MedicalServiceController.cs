using HIV_System_API_DTOs.MedicalServiceDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // This makes all endpoints require authentication by default
    public class MedicalServiceController : ControllerBase
    {
        private readonly IMedicalServiceService _medicalServiceService;

        public MedicalServiceController()
        {
            _medicalServiceService = new MedicalServiceService();
        }

        /// <summary>
        /// Get all medical services
        /// </summary>
        /// <returns>List of all medical services</returns>
        /// <response code="200">Returns the list of services</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetAllMedicalServices")]
        [AllowAnonymous] // Allow anyone to view services
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MedicalServiceResponseDTO>>> GetAllMedicalServices()
        {
            try
            {
                var services = await _medicalServiceService.GetAllMedicalServicesAsync();
                return Ok(services);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Get a specific medical service by ID
        /// </summary>
        /// <param name="id">The ID of the service to retrieve</param>
        /// <returns>The requested medical service</returns>
        /// <response code="200">Returns the requested service</response>
        /// <response code="404">If the service was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpGet("GetMedicalServiceById/{id}")]
        [AllowAnonymous] // Allow anyone to view service details
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalServiceResponseDTO>> GetMedicalServiceById(int id)
        {
            try
            {
                var service = await _medicalServiceService.GetMedicalServiceByIdAsync(id);
                if (service == null)
                {
                    return NotFound($"Medical service with ID {id} not found.");
                }
                return Ok(service);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Create a new medical service
        /// </summary>
        /// <param name="serviceDTO">The service details</param>
        /// <returns>The created service</returns>
        /// <response code="201">Returns the newly created service</response>
        /// <response code="400">If the service data is invalid</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPost("CreateMedicalService")]
        [Authorize(Roles = "1,5")] // Only admin and manager can create services
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalServiceResponseDTO>> CreateMedicalService([FromBody] MedicalServiceRequestDTO serviceDTO)
        {
            try
            {
                var createdService = await _medicalServiceService.CreateMedicalServiceAsync(serviceDTO);
                return CreatedAtAction(nameof(GetMedicalServiceById), new { id = createdService.SrvId }, createdService);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Update an existing medical service
        /// </summary>
        /// <param name="id">The ID of the service to update</param>
        /// <param name="serviceDTO">The updated service details</param>
        /// <returns>The updated service</returns>
        /// <response code="200">Returns the updated service</response>
        /// <response code="400">If the service data is invalid</response>
        /// <response code="404">If the service was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpPut("UpdateMedicalService/{id}")]
        [Authorize(Roles = "1,5")] // Only admin and manager can update services
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalServiceResponseDTO>> UpdateMedicalService(int id, [FromBody] MedicalServiceRequestDTO serviceDTO)
        {
            try
            {
                var updatedService = await _medicalServiceService.UpdateMedicalServiceAsync(id, serviceDTO);
                return Ok(updatedService);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.InnerException);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Medical service with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Delete a medical service
        /// </summary>
        /// <param name="id">The ID of the service to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">If the service was successfully deleted</response>
        /// <response code="404">If the service was not found</response>
        /// <response code="500">If there was an internal server error</response>
        [HttpDelete("DeleteMedicalService/{id}")]
        [Authorize(Roles = "1")] // Only admin can delete services
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteMedicalService(int id)
        {
            try
            {
                var result = await _medicalServiceService.DeleteMedicalServiceAsync(id);
                if (!result)
                {
                    return NotFound($"Medical service with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }
        [HttpPost("DisableMedicalService/{id}")]
        [Authorize(Roles = "1,5")] // Only admin and manager can disable services
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<MedicalServiceResponseDTO>> DisableMedicalService(int id)
        {
            try
            {
                var disabledService = await _medicalServiceService.DisableMedicalServiceAsync(id);
                if (disabledService == null)
                {
                    return NotFound($"Medical service with ID {id} not found.");
                }
                return Ok(disabledService);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}