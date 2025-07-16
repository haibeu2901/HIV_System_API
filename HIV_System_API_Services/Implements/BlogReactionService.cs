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
        private readonly IBlogReactionRepo _repo;

        public BlogReactionService(IBlogReactionRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo), "Kho lưu trữ phản hồi blog không được để trống.");
        }

        private static CommentResponseDTO MapToResponseDTO(BlogReaction reaction)
        {
            if (reaction == null)
                throw new ArgumentNullException(nameof(reaction), "Thực thể phản hồi không được để trống.");

            return new CommentResponseDTO
            {
                ReactionId = reaction.BrtId,
                BlogId = reaction.SblId,
                AccountId = reaction.AccId,
                Comment = reaction.Comment ?? string.Empty,
                ReactedAt = reaction.ReactedAt ?? DateTime.UtcNow,
            };
        }

        public async Task<CommentResponseDTO> CreateCommentAsync(CommentRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Yêu cầu DTO là bắt buộc.");

            if (dto.BlogId <= 0)
                throw new ArgumentException("ID bài viết không hợp lệ.", nameof(dto.BlogId));

            if (dto.AccountId <= 0)
                throw new ArgumentException("ID tài khoản không hợp lệ.", nameof(dto.AccountId));

            if (string.IsNullOrWhiteSpace(dto.Comment))
                throw new ArgumentException("Nội dung bình luận không được để trống.", nameof(dto.Comment));

            var entity = new BlogReaction
            {
                SblId = dto.BlogId,
                AccId = dto.AccountId,
                Comment = dto.Comment,
                ReactedAt = DateTime.UtcNow
            };

            try
            {
                var created = await _repo.CreateCommentAsync(entity);
                return MapToResponseDTO(created);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể tạo mới bình luận.", ex);
            }
        }

        public async Task<CommentResponseDTO?> GetCommentByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID bình luận không hợp lệ.", nameof(id));

            var comment = await _repo.GetCommentByIdAsync(id);
            if (comment == null)
                throw new InvalidOperationException($"Không tìm thấy bình luận với ID {id}.");

            return MapToResponseDTO(comment);
        }

        public async Task<List<CommentResponseDTO>> GetCommentsByBlogIdAsync(int blogId)
        {
            if (blogId <= 0)
                throw new ArgumentException("ID bài viết không hợp lệ.", nameof(blogId));

            try
            {
                var comments = await _repo.GetCommentsByBlogIdAsync(blogId);
                if (comments == null || !comments.Any())
                    throw new InvalidOperationException($"Không tìm thấy bình luận nào cho bài viết với ID {blogId}.");

                return comments.Select(MapToResponseDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất bình luận cho bài viết với ID {blogId}.", ex);
            }
        }

        public async Task<CommentResponseDTO> UpdateCommentAsync(int id, UpdateCommentRequestDTO dto)
        {
            if (id <= 0)
                throw new ArgumentException("ID bình luận không hợp lệ.", nameof(id));

            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Yêu cầu DTO là bắt buộc.");

            if (string.IsNullOrWhiteSpace(dto.Comment))
                throw new ArgumentException("Nội dung bình luận không được để trống.", nameof(dto.Comment));

            try
            {
                var updated = await _repo.UpdateCommentAsync(id, dto.Comment);
                if (updated == null)
                    throw new InvalidOperationException($"Không tìm thấy bình luận với ID {id} để cập nhật.");

                return MapToResponseDTO(updated);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể cập nhật bình luận với ID {id}.", ex);
            }
        }

        public async Task<bool> DeleteCommentAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("ID bình luận không hợp lệ.", nameof(id));

            try
            {
                var result = await _repo.DeleteCommentAsync(id);
                if (!result)
                    throw new InvalidOperationException($"Không tìm thấy bình luận với ID {id} để xóa.");

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể xóa bình luận với ID {id}.", ex);
            }
        }

        public async Task<BlogReactionResponseDTO> UpdateBlogReactionAsync(BlogReactionRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Yêu cầu DTO là bắt buộc.");

            if (dto.BlogId <= 0)
                throw new ArgumentException("ID bài viết không hợp lệ.", nameof(dto.BlogId));

            if (dto.AccountId <= 0)
                throw new ArgumentException("ID tài khoản không hợp lệ.", nameof(dto.AccountId));

            try
            {
                var reaction = await _repo.UpdateBlogReactionAsync(dto.BlogId, dto.AccountId, dto.ReactionType);
                if (reaction == null)
                    throw new InvalidOperationException("Không thể cập nhật phản hồi cho bài viết.");

                return new BlogReactionResponseDTO
                {
                    ReactionId = reaction.BrtId,
                    BlogId = reaction.SblId,
                    AccountId = reaction.AccId,
                    ReactionType = reaction.ReactionType,
                    ReactedAt = reaction.ReactedAt ?? DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Không thể cập nhật phản hồi cho bài viết.", ex);
            }
        }

        public async Task<int> GetReactionCountByBlogIdAsync(int blogId, bool reactionType)
        {
            if (blogId <= 0)
                throw new ArgumentException("ID bài viết không hợp lệ.", nameof(blogId));

            try
            {
                return await _repo.GetReactionCountByBlogIdAsync(blogId, reactionType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Không thể truy xuất số lượng phản hồi cho bài viết với ID {blogId}.", ex);
            }
        }
    }
}