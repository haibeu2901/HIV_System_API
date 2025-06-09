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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
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
                return StatusCode(StatusCodes.Status500InternalServerError, $"Internal server error: {ex.Message}");
            }
        }
    }
}
