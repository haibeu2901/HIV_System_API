using HIV_System_API_BOs;
using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArvMedicationDetailController : ControllerBase
    {
        private IArvMedicationDetailService _arvMedicationDetailService;

        public ArvMedicationDetailController()
        {
            _arvMedicationDetailService = new ArvMedicationDetailService();
        }

        // Use for unit testing or dependency injection
        //public ArvMedicationDetailController(IArvMedicationDetailService arvMedicationDetailService)
        //{
        //    _arvMedicationDetailService = arvMedicationDetailService; // Injected
        //}

        [HttpGet("GetAllArvMedicationDetails")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllArvMedicationDetails()
        {
            try
            {
                var arvMedicationDetails = await _arvMedicationDetailService.GetAllArvMedicationDetailsAsync();
                return Ok(arvMedicationDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetArvMedicationDetailById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetArvMedicationDetailById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("ID must be greater than zero.");
                }
                var arvMedicationDetail = await _arvMedicationDetailService.GetArvMedicationDetailByIdAsync(id);
                if (arvMedicationDetail == null)
                {
                    return NotFound($"ARV Medication Detail with ID {id} not found.");
                }
                return Ok(arvMedicationDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("CreateArvMedicationDetail")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> CreateArvMedicationDetail([FromBody] ArvMedicationDetailResponseDTO arvMedicationDetailDto)
        {
            try
            {
                if (arvMedicationDetailDto == null)
                {
                    return BadRequest("ARV Medication Detail cannot be null.");
                }
                var createdDetail = await _arvMedicationDetailService.CreateArvMedicationDetailAsync(arvMedicationDetailDto);
                if (createdDetail != null)
                {
                    // Assuming createdDetail has an ID property, otherwise adjust accordingly
                    return CreatedAtAction(nameof(GetArvMedicationDetailById), new { id = createdDetail.ARVMedicationName }, createdDetail);
                }
                return BadRequest("Failed to create ARV Medication Detail.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateArvMedicationDetail")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> UpdateArvMedicationDetail(int id, [FromBody] ArvMedicationDetailResponseDTO arvMedicationDetailDto)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("ID must be greater than zero.");
                }
                if (arvMedicationDetailDto == null)
                {
                    return BadRequest("ARV Medication Detail cannot be null.");
                }

                var existing = await _arvMedicationDetailService.GetArvMedicationDetailByIdAsync(id);
                if (existing == null)
                {
                    return NotFound($"ARV Medication Detail with ID {id} not found.");
                }

                var updatedDetail = await _arvMedicationDetailService.UpdateArvMedicationDetailAsync(id, arvMedicationDetailDto);
                if (updatedDetail != null)
                {
                    return NoContent();
                }
                return BadRequest("Failed to update ARV Medication Detail.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteArvMedicationDetail")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> DeleteArvMedicationDetail(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("ID must be greater than zero.");
                }
                var result = await _arvMedicationDetailService.DeleteArvMedicationDetailAsync(id);
                if (result)
                {
                    return NoContent();
                }
                return NotFound($"ARV Medication Detail with ID {id} not found.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("SearchArvMedicationDetailsByName")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchArvMedicationDetailsByName(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("Search term cannot be null or empty.");
                }
                var results = await _arvMedicationDetailService.SearchArvMedicationDetailsByNameAsync(searchTerm);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
