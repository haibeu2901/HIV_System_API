using HIV_System_API_DTOs.ARVRegimenTemplateDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegimenTemplateController : ControllerBase
    {
        private readonly IRegimenTemplateService _regimenTemplateService;

        public RegimenTemplateController(IRegimenTemplateService regimenTemplateService)
        {
            _regimenTemplateService = regimenTemplateService;
        }

        [HttpGet("GetAllRegimenTemplates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllRegimenTemplates()
        {
            try
            {
                var templates = await _regimenTemplateService.GetAllRegimenTemplatesAsync();
                return Ok(templates);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpGet("GetRegimenTemplateById")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegimenTemplateById(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid regimen template ID.");
            try
            {
                var template = await _regimenTemplateService.GetRegimenTemplateByIdAsync(id);
                if (template == null)
                    return NotFound($"Regimen template with ID {id} not found.");

                return Ok(template);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpPost("CreateRegimenTemplate")]
        [Authorize(Roles = "1, 5")]
        public async Task<IActionResult> CreateRegimenTemplate([FromBody] RegimenTemplateRequestDTO request)
        {
            if (request == null)
                return BadRequest("Regimen template data is required.");

            try
            {
                var createdTemplate = await _regimenTemplateService.CreateRegimenTemplateAsync(request);
                if (createdTemplate == null)
                    return StatusCode(500, "Failed to create regimen template.");

                return CreatedAtAction(nameof(GetRegimenTemplateById), new { id = createdTemplate.ArtId }, createdTemplate);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpDelete("DeleteRegimenTemplate")]

        [Authorize(Roles = "1, 5")]
        public async Task<IActionResult> DeleteRegimenTemplate(int id)
        {
            if (id <= 0)
                return BadRequest("Invalid regimen template ID.");

            try
            {
                var deleted = await _regimenTemplateService.DeleteRegimenTemplateAsync(id);
                if (!deleted)
                    return NotFound($"Regimen template with ID {id} not found or could not be deleted.");
                return NoContent();
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpPut("UpdateRegimenTemplate")]
        [Authorize(Roles = "1, 5")]
        public async Task<IActionResult> UpdateRegimenTemplate(int id, [FromBody] RegimenTemplateRequestDTO request)
        {
            if (id <= 0 || request == null)
                return BadRequest("Invalid regimen template data.");
            try
            {
                var updatedTemplate = await _regimenTemplateService.UpdateRegimenTemplateAsync(id, request);
                if (updatedTemplate == null)
                    return NotFound($"Regimen template with ID {id} not found or could not be updated.");
                return Ok(updatedTemplate);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpGet("GetRegimenTemplateByDescription")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegimenTemplateByDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return BadRequest("Description is required.");
            try
            {
                var templates = await _regimenTemplateService.GetRegimenTemplatesByDescriptionAsync(description);
                if (templates == null || !templates.Any())
                    return NotFound($"No regimen templates found with description '{description}'.");
                return Ok(templates);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }

        [HttpGet("GetRegimenTemplatesByLevel")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRegimenTemplatesByLevel(int level)
        {
            if (level < 1 || level > 4)
                return BadRequest("Level must be between 1 and 4.");

            try
            {
                var templates = await _regimenTemplateService.GetRegimenTemplatesByLevelAsync((byte)level);
                if (templates == null || !templates.Any())
                    return NotFound($"No regimen templates found for level {level}.");
                return Ok(templates);
            }
            catch (Exception ex)
            {
                var errorMessage = ex.InnerException != null
                    ? $"Internal server error: {ex.Message} | Inner exception: {ex.InnerException.Message}"
                    : $"Internal server error: {ex.Message}";
                return StatusCode(500, errorMessage);
            }
        }
    }
}
