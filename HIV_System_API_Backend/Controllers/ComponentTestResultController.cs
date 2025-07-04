using HIV_System_API_Services.Implements;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HIV_System_API_DTOs.ComponentTestResultDTO;

namespace HIV_System_API_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComponentTestResultController : ControllerBase
    {
        private readonly IComponentTestResultService _componentTestResultService;

        public ComponentTestResultController()
        {
            _componentTestResultService = new ComponentTestResultService();
        }

        [HttpGet("GetAll")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<List<ComponentTestResultResponseDTO>>> GetAllTestComponent()
        {
            var components = await _componentTestResultService.GetAllTestComponent();
            return Ok(components);
        }

        [HttpGet("GetById/{id:int}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<ComponentTestResultResponseDTO>> GetTestComponentById(int id)
        {
            try
            {
                var component = await _componentTestResultService.GetTestComponentById(id);
                return Ok(component);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("Create")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<ComponentTestResultResponseDTO>> AddTestComponent([FromBody] ComponentTestResultRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Component test result data is required.");

            try
            {
                var result = await _componentTestResultService.AddTestComponent(dto);
                return CreatedAtAction(nameof(GetTestComponentById), new { id = result.ComponentTestResultId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update/{id:int}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<ActionResult<ComponentTestResultResponseDTO>> UpdateTestComponent(int id, [FromBody] ComponentTestResultUpdateRequestDTO dto)
        {
            if (dto == null)
                return BadRequest("Update data is required.");

            try
            {
                var result = await _componentTestResultService.UpdateTestComponent(id, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "1,2,4,5")]
        public async Task<IActionResult> DeleteTestComponent(int id)
        {
            try
            {
                var deleted = await _componentTestResultService.DeleteTestComponent(id);
                if (!deleted)
                    return NotFound($"Component test result with ID {id} not found.");
                return NoContent();
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
