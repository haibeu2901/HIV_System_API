using HIV_System_API_BOs;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
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

        [HttpGet("GetAllArvMedicationDetails")]
        public async Task<IActionResult> GetAllArvMedicationDetails()
        {
            try
            {
                var arvMedicationDetails = await _arvMedicationDetailService.GetAllArvMedicationDetailsAsync();
                return Ok(arvMedicationDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("GetArvMedicationDetailById/{id}")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPost("CreateArvMedicationDetail")]
        public async Task<IActionResult> CreateArvMedicationDetail([FromBody] ArvMedicationDetail arvMedicationDetail)
        {
            try
            {
                if (arvMedicationDetail == null)
                {
                    return BadRequest("ARV Medication Detail cannot be null.");
                }
                var result = await _arvMedicationDetailService.CreateArvMedicationDetailAsync(arvMedicationDetail);
                if (result)
                {
                    return CreatedAtAction(nameof(GetArvMedicationDetailById), new { id = arvMedicationDetail.AmdId }, arvMedicationDetail);
                }
                return BadRequest("Failed to create ARV Medication Detail.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpPut("UpdateArvMedicationDetail/{id}")]
        public async Task<IActionResult> UpdateArvMedicationDetail(int id, [FromBody] ArvMedicationDetail arvMedicationDetail)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest("ID must be greater than zero.");
                }
                if (arvMedicationDetail == null)
                {
                    return BadRequest("ARV Medication Detail cannot be null.");
                }
                if (id != arvMedicationDetail.AmdId)
                {
                    return BadRequest("ID in URL does not match ID in body.");
                }

                var existing = await _arvMedicationDetailService.GetArvMedicationDetailByIdAsync(id);
                if (existing == null)
                {
                    return NotFound($"ARV Medication Detail with ID {id} not found.");
                }

                // Update the properties of the existing detail with the new values
                existing.AmdId = id;
                existing.MedName = arvMedicationDetail.MedName;
                existing.Dosage = arvMedicationDetail.Dosage;
                existing.MedDescription = arvMedicationDetail.MedDescription;
                existing.Price = arvMedicationDetail.Price;
                existing.Manufactorer = arvMedicationDetail.Manufactorer;

                var result = await _arvMedicationDetailService.UpdateArvMedicationDetailAsync(id, existing);
                if (result)
                {
                    return NoContent();
                }
                return BadRequest("Failed to update ARV Medication Detail.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpDelete("DeleteArvMedicationDetail/{id}")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }

        [HttpGet("SearchArvMedicationDetailsByName/{searchTerm}")]
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.InnerException}");
            }
        }
    }
}
