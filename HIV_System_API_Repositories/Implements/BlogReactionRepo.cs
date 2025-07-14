using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class BlogReactionRepo
    {
        private readonly BlogReactionDAO _dao = new();
        public Task<BlogReaction> CreateCommentAsync(BlogReaction comment) => _dao.CreateCommentAsync(comment);
        public Task<BlogReaction?> GetCommentByIdAsync(int id) => _dao.GetCommentByIdAsync(id);
        public Task<List<BlogReaction>> GetCommentsByBlogIdAsync(int blogId) => _dao.GetCommentsByBlogIdAsync(blogId);
        public Task<BlogReaction> UpdateCommentAsync(int id, string comment) => _dao.UpdateCommentAsync(id, comment);
        public Task<bool> DeleteCommentAsync(int id) => _dao.DeleteCommentAsync(id);
    }
}
