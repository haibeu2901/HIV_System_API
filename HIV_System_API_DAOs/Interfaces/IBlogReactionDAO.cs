using HIV_System_API_BOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IBlogReactionDAO
    {
        Task<BlogReaction> CreateCommentAsync(BlogReaction comment);
        Task<BlogReaction?> GetCommentByIdAsync(int id);
        Task<List<BlogReaction>> GetCommentsByBlogIdAsync(int blogId);
        Task<BlogReaction> UpdateCommentAsync(int id, string comment);
        Task<bool> DeleteCommentAsync(int id);
    }
}