using HIV_System_API_DTOs.BlogReactionDTO;
using HIV_System_API_Services.Implements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogReactionController : ControllerBase
    {
        private readonly BlogReactionService _service = new();

        [HttpPost("CreateComment")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<IActionResult> Create([FromBody] CommentRequestDTO dto)
        {
            var result = await _service.CreateCommentAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.ReactionId }, result);
        }

        [HttpGet("GetCommentByID/{id}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetCommentByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpGet("GetCommentByBlogID/{blogId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByBlogId(int blogId)
        {
            var results = await _service.GetCommentsByBlogIdAsync(blogId);
            return Ok(results);
        }

        [HttpPut("UpdateCommentByID/{id}")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentRequestDTO dto)
        {
            var result = await _service.UpdateCommentAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("DeleteCommentByID/{id}")]
        [Authorize(Roles = "1,4")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteCommentAsync(id);
            return success ? NoContent() : NotFound();
        }

        [HttpPost("UpdateBlogReaction")]
        [Authorize(Roles = "1,2,3,4,5")]
        public async Task<IActionResult> UpdateBlogReaction([FromBody] BlogReactionRequestDTO dto)
        {
            var result = await _service.UpdateBlogReactionAsync(dto);
            return Ok(result);
        }

        [HttpGet("GetReactionCountByBlogID/{blogId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetReactionCountByBlogId(int blogId, [FromQuery] bool reactionType)
        {
            var count = await _service.GetReactionCountByBlogIdAsync(blogId, reactionType);
            return Ok(count);
        }
    }
}