using HIV_System_API_BOs;
using HIV_System_API_DTOs.BlogReactionDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class BlogReactionService : IBlogReactionService
    {
        private readonly BlogReactionRepo _repo = new BlogReactionRepo();
        private static CommentResponseDTO MapToResponseDTO(BlogReaction reaction) =>
            new()
            {
                ReactionId = reaction.BrtId,
                BlogId = reaction.SblId,
                AccountId = reaction.AccId,
                Comment = reaction.Comment ?? string.Empty,
                ReactedAt = reaction.ReactedAt ?? DateTime.UtcNow,
            };
        public async Task<CommentResponseDTO> CreateCommentAsync(CommentRequestDTO dto)
        {
            var entity = new BlogReaction
            {
                SblId = dto.BlogId,
                AccId = dto.AccountId,
                Comment = dto.Comment,
                ReactedAt = DateTime.UtcNow
            };
            var created = await _repo.CreateCommentAsync(entity);
            return MapToResponseDTO(created);
        }

        public async Task<CommentResponseDTO?> GetCommentByIdAsync(int id)
        {
            var comment = await _repo.GetCommentByIdAsync(id);
            return comment == null ? null : MapToResponseDTO(comment);
        }

        public async Task<List<CommentResponseDTO>> GetCommentsByBlogIdAsync(int blogId)
        {
            var comments = await _repo.GetCommentsByBlogIdAsync(blogId);
            return comments.Select(MapToResponseDTO).ToList();
        }

        public async Task<CommentResponseDTO> UpdateCommentAsync(int id, UpdateCommentRequestDTO dto)
        {
            var updated = await _repo.UpdateCommentAsync(id, dto.Comment);
            return MapToResponseDTO(updated);
        }

        public Task<bool> DeleteCommentAsync(int id) => _repo.DeleteCommentAsync(id);

        public async Task<BlogReactionResponseDTO> UpdateBlogReactionAsync(BlogReactionRequestDTO dto)
        {
            var reaction = await _repo.UpdateBlogReactionAsync(dto.BlogId, dto.AccountId, dto.ReactionType);
            return new BlogReactionResponseDTO
            {
                ReactionId = reaction.BrtId,
                BlogId = reaction.SblId,
                AccountId = reaction.AccId,
                ReactionType = reaction.ReactionType,
                ReactedAt = reaction.ReactedAt ?? DateTime.UtcNow
            };
        }
        public async Task<int> GetReactionCountByBlogIdAsync(int blogId, bool reactionType)
        {
            return await _repo.GetReactionCountByBlogIdAsync(blogId, reactionType);
        }
    }
}
