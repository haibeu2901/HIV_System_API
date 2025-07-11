using HIV_System_API_DTOs.StaffDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;

        public StaffController()
        {
            _staffService = new StaffService();
        }

        // <summary>
        /// Gets all staff members
        /// </summary>
        /// <returns>List of all staff members</returns>
        [HttpGet("GetAllStaffs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllStaffs()
        {
            try
            {
                var staffs = await _staffService.GetAllStaffsAsync();

                if (staffs == null || staffs.Count == 0)
                {
                    return Ok(new List<StaffResponseDTO>()); // Return empty list with 200 OK
                }

                return Ok(staffs);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving staffs: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred: {ex.InnerException}");
            }
        }

        // <summary>
        /// Gets a specific staff member by ID
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>Staff member details</returns>
        [HttpGet("GetStaffById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStaffById(int id)
        {
            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id);

                if (staff == null)
                {
                    return NotFound($"Staff with ID {id} not found.");
                }

                return Ok(staff);
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error retrieving staff: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Creates a new staff member
        /// </summary>
        /// <param name="staffRequest">Staff member data</param>
        /// <returns>Created staff member</returns>
        [HttpPost("CreateStaff")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> CreateStaff([FromBody] StaffRequestDTO staffRequest)
        {
            if (staffRequest == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var createdStaff = await _staffService.CreateStaffAsync(staffRequest);

                return CreatedAtAction(
                    nameof(GetStaffById),
                    new { id = createdStaff.StaffId },
                    createdStaff);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while creating staff: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error creating staff: {ex.InnerException}");
            }
        }

        // <summary>
        /// Updates an existing staff member
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <param name="staffRequest">Updated staff member data</param>
        /// <returns>Updated staff member</returns>
        [HttpPut("UpdateStaff/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] StaffRequestDTO staffRequest)
        {
            if (staffRequest == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var updatedStaff = await _staffService.UpdateStaffAsync(id, staffRequest);

                if (updatedStaff == null)
                {
                    return NotFound($"Staff with ID {id} not found.");
                }

                return Ok(updatedStaff);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while updating staff: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error updating staff: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Deletes a staff member
        /// </summary>
        /// <param name="id">Staff ID</param>
        /// <returns>NoContent if successful</returns>
        [HttpDelete("DeleteStaff/{id}")]
        [Authorize(Roles = "1")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            try
            {
                var result = await _staffService.DeleteStaffAsync(id);

                if (!result)
                {
                    return NotFound($"Staff with ID {id} not found.");
                }

                return NoContent(); // 204 No Content
            }
            catch (ArgumentException ex)
            {
                return BadRequest($"Invalid input: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest($"Operation failed: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Database error while deleting staff: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error deleting staff: {ex.InnerException}");
            }
        }

        /// <summary>
        /// Searches for staff members by search term
        /// </summary>
        /// <param name="searchTerm">Search term to filter staff members</param>
        /// <returns>List of staff members matching the search term</returns>
        [HttpGet("SearchStaffs")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchStaffs([FromQuery] string searchTerm)
        {
            try
            {
                // If searchTerm is null, set it to empty string to get all records
                searchTerm = searchTerm ?? string.Empty;

                var staffs = await _staffService.GetStaffsBySearchAsync(searchTerm);

                if (staffs == null || staffs.Count == 0)
                {
                    return Ok(new List<StaffResponseDTO>()); // Return empty list with 200 OK
                }

                return Ok(staffs);
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest($"Missing required data: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Error searching staffs: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"Unexpected error occurred while searching staffs: {ex.InnerException}");
            }
        }
    }
}
