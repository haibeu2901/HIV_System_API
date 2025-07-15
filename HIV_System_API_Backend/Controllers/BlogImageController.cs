using HIV_System_API_DTOs.BlogImageDTO;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HIV_System_API_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogImageController : ControllerBase
    {
        private readonly IBlogImageService _blogImageService;

        public BlogImageController(IBlogImageService blogImageService)
        {
            _blogImageService = blogImageService;
        }

        [HttpPost("UploadImage")]
        [Authorize (Roles = "1, 2, 3, 4, 5")]
        public async Task<ActionResult<BlogImageResponseDTO>> UploadImage([FromForm] BlogImageRequestDTO request)
        {
            try
            {
                var blogImageResponseDto = await _blogImageService.UploadBlogImageAsync(request);
                return Ok(blogImageResponseDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetImageByID/{imgId}")]
        [AllowAnonymous]
        public async Task<ActionResult<BlogImageResponseDTO>> GetImage(int imgId)
        {
            try
            {
                var blogImageResponseDto = await _blogImageService.GetByIdAsync(imgId);
                return Ok(blogImageResponseDto);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
        [HttpDelete("DeleteImage/{imgId}")]
        [Authorize(Roles = "1, 2, 3, 4, 5")]
        public async Task<IActionResult> DeleteImage(int imgId)
        {
            try
            {
                var success = await _blogImageService.DeleteBlogImageAsync(imgId);
                if (success)
                {
                    return Ok("Image deleted successfully");
                }
                return NotFound("Image not found");
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetImagesByBlogId/{sblId}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<BlogImageResponseDTO>>> GetByBlogId(int sblId)
        {
            try
            {
                var blogImageResponseDtos = await _blogImageService.GetByBlogIdAsync(sblId);
                return Ok(blogImageResponseDtos);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
