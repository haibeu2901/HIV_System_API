using HIV_System_API_DTOs.SocialBlogDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace HIV_System_API_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocialBlogController : ControllerBase
    {
        private readonly ISocialBlogService _service;

        public SocialBlogController(ISocialBlogService service)
        {
            _service = service;
        }

        [HttpGet("GetAllBlog")]
        [AllowAnonymous]
        public async Task<ActionResult<List<BlogResponseDTO>>> GetAll()
        {
            try
            {
                var blogs = await _service.GetAllAsync();
                return Ok(blogs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("GetBlogById/{id}")]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<BlogResponseDTO>> GetById(int id)
        {
            try
            {
                var blog = await _service.GetByIdAsync(id);
                if (blog == null)
                    return NotFound(new { message = $"Blog with ID {id} not found." });
                return Ok(blog);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("CreateBlog")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<BlogResponseDTO>> Create([FromBody] BlogCreateRequestDTO request)
        {
            try
            {
                var created = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("UpdateBlog/{id}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<ActionResult<BlogResponseDTO>> Update(int id, [FromBody] BlogUpdateRequestDTO request)
        {
            try
            {
                var updated = await _service.UpdateAsync(id, request);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("DeleteBlog/{id}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = $"Blog with ID {id} not found." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPatch("VerifiBlog/{id}")]
        [Authorize(Roles = "1,4")]
        public async Task<ActionResult<BlogResponseDTO>> UpdateBlogStatusAsync(int id, [FromBody] BlogVerificationRequestDTO request)
        {
            try
            {
                var staffIdClaim = User.FindFirst("AccountId")?.Value;
                if (!int.TryParse(staffIdClaim, out int staffId))
                    return Unauthorized(new { message = "Invalid staff credentials" });

                // Set the staff ID from the authenticated user
                request.StaffId = staffId;

                var verified = await _service.UpdateBlogStatusAsync(id, request);
                return Ok(verified);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPost("SaveBlogAsDraft")]
        public async Task<ActionResult<BlogResponseDTO>> SaveDraft([FromBody] BlogCreateRequestDTO request)
        {


            try
            {
                var created = await _service.CreateDraftAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }

        [HttpGet("GetAllDraftsById/{Id}")]
        public async Task<ActionResult<List<BlogResponseDTO>>> GetUserDrafts(int authorId)
        {
            try
            {
                var drafts = await _service.GetDraftsByAuthorIdAsync(authorId);
                if (drafts == null)
                    return NotFound(new { message = $"Blog with ID {authorId} not found." });
                return Ok(drafts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }

        }
    }
}