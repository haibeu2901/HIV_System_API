using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class SocialBlogRepo : ISocialBlogRepo
    {
        public async Task<List<SocialBlog>> GetAllAsync()
        {
            return await SocialBlogDAO.Instance.GetAllAsync();
        }

        public async Task<SocialBlog?> GetByIdAsync(int id)
        {
            return await SocialBlogDAO.Instance.GetByIdAsync(id);
        }

        public async Task<List<SocialBlog?>> GetByAuthorIdAsync(int id)
        {
            return await SocialBlogDAO.Instance.GetByAuthorIdAsync(id);
        }

        public async Task<SocialBlog> CreateAsync(SocialBlog blog)
        {
            return await SocialBlogDAO.Instance.CreateAsync(blog);
        }

        public async Task<SocialBlog> UpdateAsync(int id, SocialBlog blog)
        {
            return await SocialBlogDAO.Instance.UpdateAsync(id, blog);
        }
        public async Task<SocialBlog> UpdatePersonalAsync(int blogId, int authorId, SocialBlog blog)
        {
            return await SocialBlogDAO.Instance.UpdatePersonalAsync(blogId, authorId, blog);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await SocialBlogDAO.Instance.DeleteAsync(id);
        }

        public async Task<SocialBlog> UpdateBlogStatusAsync(int id, byte blogStatus, int staffId, string? notes)
        {
            return await SocialBlogDAO.Instance.UpdateBlogStatusAsync(id, blogStatus, staffId, notes);
        }
        public async Task<SocialBlog> CreateDraftAsync(SocialBlog draft)
        {
            return await SocialBlogDAO.Instance.CreateDraftAsync(draft);
        }
        public async Task<List<SocialBlog>> GetDraftsByAuthorIdAsync(int authorId)
        {
            return await SocialBlogDAO.Instance.GetDraftsByAuthorIdAsync(authorId);
        }
    }
}