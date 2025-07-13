using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIV_System_API_DAOs.Implements
{
    public class BlogReactionDAO : IBlogReactionDAO
    {
        private readonly HivSystemApiContext _context;
        public BlogReactionDAO() => _context = new HivSystemApiContext();

        public async Task<BlogReaction> CreateCommentAsync(BlogReaction comment)
        {
            await _context.BlogReactions.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<BlogReaction?> GetCommentByIdAsync(int id)
        {
            return await _context.BlogReactions.FindAsync(id);
        }

        public async Task<List<BlogReaction>> GetCommentsByBlogIdAsync(int blogId)
        {
            return await _context.BlogReactions
                .Where(r => r.SblId == blogId && r.Comment != null)
                .ToListAsync();
        }

        public async Task<BlogReaction> UpdateCommentAsync(int id, string comment)
        {
            var reaction = await _context.BlogReactions.FindAsync(id);
            if (reaction == null) throw new KeyNotFoundException($"Comment with id {id} not found.");
            reaction.Comment = comment;
            await _context.SaveChangesAsync();
            return reaction;
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            var reaction = await _context.BlogReactions.FindAsync(id);
            if (reaction == null) return false;
            _context.BlogReactions.Remove(reaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}