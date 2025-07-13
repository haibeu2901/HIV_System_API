using HIV_System_API_DTOs.BlogReactionDTO;
using HIV_System_API_Services.Implements;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogReactionController : ControllerBase
    {
        private readonly BlogReactionService _service = new();

        [HttpPost("CreateComment")]
        public async Task<IActionResult> Create([FromBody] CommentRequestDTO dto)
        {
            var result = await _service.CreateCommentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ReactionId }, result);
        }

        [HttpGet("GetCommentByID/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetCommentByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("GetCommentByBlogID/{blogId}")]
        public async Task<IActionResult> GetByBlogId(int blogId)
        {
            var results = await _service.GetCommentsByBlogIdAsync(blogId);
            return Ok(results);
        }

        [HttpPut("UpdateCommentByID/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentRequestDTO dto)
        {
            var result = await _service.UpdateCommentAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("DeleteCommentByID/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteCommentAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}