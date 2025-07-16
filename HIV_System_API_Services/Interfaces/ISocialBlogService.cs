using HIV_System_API_DTOs.SocialBlogDTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Interfaces
{
    public interface ISocialBlogService
    {
        Task<List<BlogResponseDTO>> GetAllAsync();
        Task<BlogResponseDTO?> GetByIdAsync(int id);
        Task<BlogResponseDTO?> GetByAuthorIdAsync(int authorId);
        Task<BlogResponseDTO> CreateAsync(BlogCreateRequestDTO request);
        Task<BlogResponseDTO> UpdateAsync(int id, BlogUpdateRequestDTO request);
        Task<BlogResponseDTO> UpdatePersonalAsync(int blogId, int authorId, BlogUpdateRequestDTO request);
        Task<bool> DeleteAsync(int id);
        Task<BlogResponseDTO> UpdateBlogStatusAsync(int id, BlogVerificationRequestDTO request);
        Task<BlogResponseDTO> CreateDraftAsync(BlogCreateRequestDTO request);
        Task<List<BlogResponseDTO>> GetDraftsByAuthorIdAsync(int authorId);
    }
}