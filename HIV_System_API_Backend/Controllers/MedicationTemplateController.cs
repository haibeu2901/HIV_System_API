using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicationTemplateController : ControllerBase
    {
        private readonly IMedicationTemplateService _medicationTemplateService;

        public MedicationTemplateController()
        {
            _medicationTemplateService = new MedicationTemplateService();
        }

        [HttpGet("GetAllMedicationTemplates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllMedicationTemplates()
        {
            try
            {
                var templates = await _medicationTemplateService.GetAllMedicationTemplatesAsync();
                if (templates == null || templates.Count == 0)
                {
                    return NotFound("No medication templates found.");
                }
                return Ok(templates);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        [HttpGet("GetMedicationTemplateById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedicationTemplateById(int id)
        {
            try
            {
                var template = await _medicationTemplateService.GetMedicationTemplateByIdAsync(id);
                if (template == null)
                {
                    return NotFound($"Medication template with ID {id} not found.");
                }
                return Ok(template);
            }
            catch (ArgumentException argEx)
            {
                var errorMessage = $"Argument exception: {argEx.Message}";
                if (argEx.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {argEx.InnerException.Message}";
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        [HttpPost("CreateMedicationTemplate")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> CreateMedicationTemplate([FromBody] MedicationTemplateRequestDTO medicationTemplate)
        {
            if (medicationTemplate == null)
            {
                return BadRequest("Medication template data is required.");
            }

            try
            {
                var createdTemplate = await _medicationTemplateService.CreateMedicationTemplateAsync(medicationTemplate);
                if (createdTemplate == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create medication template.");
                }
                return CreatedAtAction(nameof(GetMedicationTemplateById), new { id = createdTemplate.ArvMedicationTemplateId }, createdTemplate);
            }
            catch (ArgumentException argEx)
            {
                var errorMessage = $"Argument exception: {argEx.Message}";
                if (argEx.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {argEx.InnerException.Message}";
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error creating medication template: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        [HttpPut("UpdateMedicationTemplate")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> UpdateMedicationTemplate([FromBody] MedicationTemplateRequestDTO medicationTemplate)
        {
            if (medicationTemplate == null)
            {
                return BadRequest("Medication template data is required.");
            }

            try
            {
                var updatedTemplate = await _medicationTemplateService.UpdateMedicationTemplateAsync(medicationTemplate);
                if (updatedTemplate == null)
                {
                    return NotFound($"Medication template with ID {medicationTemplate.ArtId} not found.");
                }
                return Ok(updatedTemplate);
            }
            catch (ArgumentException argEx)
            {
                var errorMessage = $"Argument exception: {argEx.Message}";
                if (argEx.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {argEx.InnerException.Message}";
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error updating medication template: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        [HttpDelete("DeleteMedicationTemplate")]
        [Authorize(Roles = "1,5")]
        public async Task<IActionResult> DeleteMedicationTemplate(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid MedicationTemplate ID.");
            }

            try
            {
                var result = await _medicationTemplateService.DeleteMedicationTemplateAsync(id);
                if (!result)
                {
                    return NotFound($"Medication template with ID {id} not found.");
                }
                return NoContent(); // 204 No Content
            }
            catch (ArgumentException argEx)
            {
                var errorMessage = $"Argument exception: {argEx.Message}";
                if (argEx.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {argEx.InnerException.Message}";
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error deleting medication template: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        [HttpGet("GetMedicationTemplateByArtId")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedicationTemplateByArtId(int artId)
        {
            if (artId <= 0)
            {
                return BadRequest("Invalid ArvRegimenTemplateId (ArtId).");
            }

            try
            {
                var templates = await _medicationTemplateService.GetMedicationTemplatesByArtIdAsync(artId);
                if (templates == null || templates.Count == 0)
                {
                    return NotFound($"No medication templates found for ArvRegimenTemplateId {artId}.");
                }
                return Ok(templates);
            }
            catch (ArgumentException argEx)
            {
                var errorMessage = $"Argument exception: {argEx.Message}";
                if (argEx.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {argEx.InnerException.Message}";
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        [HttpGet("GetMedicationTemplateByAmdId")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMedicationTemplateByAmdId(int amdId)
        {
            if (amdId <= 0)
            {
                return BadRequest("Invalid ArvMedicationDetailId (AmdId).");
            }

            try
            {
                var templates = await _medicationTemplateService.GetMedicationTemplatesByAmdIdAsync(amdId);
                if (templates == null || templates.Count == 0)
                {
                    return NotFound($"No medication templates found for ArvMedicationDetailId {amdId}.");
                }
                return Ok(templates);
            }
            catch (ArgumentException argEx)
            {
                var errorMessage = $"Argument exception: {argEx.Message}";
                if (argEx.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {argEx.InnerException.Message}";
                }
                return BadRequest(errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" | Inner exception: {ex.InnerException.Message}";
                }
                return StatusCode(StatusCodes.Status500InternalServerError, errorMessage);
            }
        }
    }
}
