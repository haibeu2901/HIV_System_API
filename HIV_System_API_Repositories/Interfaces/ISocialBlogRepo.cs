using HIV_System_API_BOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Interfaces
{
    public interface ISocialBlogRepo
    {
        Task<List<SocialBlog>> GetAllAsync();
        Task<SocialBlog?> GetByIdAsync(int id);
        Task<List<SocialBlog?>> GetByAuthorIdAsync(int id);
        Task<SocialBlog> CreateAsync(SocialBlog blog);
        Task<SocialBlog> UpdateAsync(int id, SocialBlog blog);
        Task<SocialBlog> UpdatePersonalAsync(int blogId, int authorId, SocialBlog blog);
        Task<bool> DeleteAsync(int id);
        Task<SocialBlog> UpdateBlogStatusAsync(int id, byte blogStatus, int staffId, string? notes);
        // Draft-specific
        Task<SocialBlog> CreateDraftAsync(SocialBlog draft);
        Task<List<SocialBlog>> GetDraftsByAuthorIdAsync(int authorId);
    }
}