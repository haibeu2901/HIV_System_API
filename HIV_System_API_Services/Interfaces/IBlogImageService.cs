using HIV_System_API_BOs;
using HIV_System_API_DTOs.BlogImageDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface IBlogImageService
    {
        Task<BlogImageResponseDTO> UploadBlogImageAsync(BlogImageRequestDTO request);
        Task<BlogImageResponseDTO> GetByIdAsync(int imgId);
        Task<List<BlogImageResponseDTO>> GetByBlogIdAsync(int blogId);
        Task<bool> DeleteBlogImageAsync(int imgId);
        Task<List<BlogImageResponseDTO>> UploadImageListAsync(BlogImageListRequestDTO blogImages);
    }
}
