using HIV_System_API_BOs;
using HIV_System_API_DTOs.SocialBlogDTO;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using HIV_System_API_DTOs.NotificationDTO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_DTOs.BlogReactionDTO;

namespace HIV_System_API_Services.Implements
{
    public class SocialBlogService : ISocialBlogService
    {
        private readonly ISocialBlogRepo _repo;
        private readonly INotificationRepo _notificationRepo;
        private readonly HivSystemApiContext _context;
        private readonly IBlogReactionRepo _blogReactionRepo;
        private readonly IAccountRepo _accountRepo;

        public SocialBlogService()
        {
            _repo = new SocialBlogRepo();
            _notificationRepo = new NotificationRepo();
            _context = new HivSystemApiContext();
            _blogReactionRepo = new BlogReactionRepo();
            _accountRepo = new AccountRepo();
        }

        // --- Mapping and Validation ---

        private SocialBlog MapToEntity(BlogCreateRequestDTO dto)
        {
            return new SocialBlog
            {
                AccId = dto.AuthorId,
                Title = dto.Title,
                Content = dto.Content,
                IsAnonymous = dto.IsAnonymous,
                Notes = dto.Notes,
                BlogStatus = 1, // Default to active/published
                PublishedAt = DateTime.UtcNow
            };
        }

        private BlogResponseDTO MapToResponseDto(SocialBlog blog)
        {
            var blogReaction = _context.BlogReactions.Where(a=>a.SblId == blog.SblId).Select(a=> new FullBlogReactionResponseDTO
            {
                ReactionId = a.BrtId,
                BlogId = a.SblId,
                AccountId = a.AccId,
                ReactedAt = a.ReactedAt,
                Comment = a.Comment,
                ReactionType = a.ReactionType
            }).ToList();
            var like = _blogReactionRepo.GetReactionCountByBlogIdAsync(blog.SblId,true).Result;
            var dislike = _blogReactionRepo.GetReactionCountByBlogIdAsync(blog.SblId, false).Result;
            var authorName = _accountRepo.GetAccountByIdAsync(blog.AccId).Result?.Fullname ?? "Unknown Author";
            return new BlogResponseDTO
            {
                Id = blog.SblId,
                AuthorId = blog.AccId,
                StaffId = blog.StfId,
                Title = blog.Title,
                Content = blog.Content,
                PublishedDate = blog.PublishedAt,
                IsAnonymous = blog.IsAnonymous,
                Notes = blog.Notes,
                BlogStatus = blog.BlogStatus,
                BlogReaction = blogReaction,
                LikesCount = like,
                DislikesCount = dislike,
                AuthorName = authorName
            };
        }

        private static void ValidateCreateRequest(BlogCreateRequestDTO dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.AuthorId <= 0)
                throw new ArgumentException("AuthorId is required and must be positive.");
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(dto.Content))
                throw new ArgumentException("Content is required.");
            if (dto.Title.Length > 200)
                throw new ArgumentException("Title exceeds maximum length of 200 characters.");
            if (dto.Content.Length > 5000)
                throw new ArgumentException("Content exceeds maximum length of 5000 characters.");
        }

        public async Task<List<BlogResponseDTO>> GetAllAsync()
        {
            var blogs = await _repo.GetAllAsync();
            return blogs.Select(MapToResponseDto).ToList();
        }

        public async Task<BlogResponseDTO?> GetByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid blog ID");

            var blog = await _repo.GetByIdAsync(id);
            return blog == null ? null : MapToResponseDto(blog);
        }
        public async Task<List<BlogResponseDTO>> GetByAuthorIdAsync(int authorId)
        {
            if (authorId <= 0)
                throw new ArgumentException("ID tác giả không hợp lệ");
            var blogs = await _repo.GetByAuthorIdAsync(authorId);
            return blogs.Select(MapToResponseDto).ToList();
        }

        public async Task<BlogResponseDTO> CreateAsync(BlogCreateRequestDTO request)
        {
            ValidateCreateRequest(request);

            var blog = new SocialBlog
            {
                AccId = request.AuthorId,
                StfId = null,
                Title = request.Title,
                Content = request.Content,
                IsAnonymous = request.IsAnonymous,
                Notes = request.Notes,
                BlogStatus = 2, // Pending for verification
                PublishedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateAsync(blog);

            // Send notification to all staff (role = 1)
            var notification = new Notification
            {
                NotiType = "Blog Pending",
                NotiMessage = $"A new blog \"{blog.Title}\" is pending verification.",
                SendAt = DateTime.UtcNow
            };
            var noti = await _notificationRepo.CreateNotificationAsync(notification);

            // Send notification to all staff accounts (role = 4)
            await _notificationRepo.SendNotificationToRoleAsync(noti.NtfId, 4);

            return MapToResponseDto(created);
        }

        public async Task<BlogResponseDTO> UpdateAsync(int id, BlogUpdateRequestDTO request)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid blog ID");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Update only provided fields
            if (request.Title != null) existing.Title = request.Title;
            if (request.Content != null) existing.Content = request.Content;
            if (request.IsAnonymous.HasValue) existing.IsAnonymous = request.IsAnonymous;
            if (request.Notes != null) existing.Notes = request.Notes;

            var updated = await _repo.UpdateAsync(id, existing);
            return MapToResponseDto(updated);
        }
        public async Task<BlogResponseDTO> UpdatePersonalAsync(int blogId, int authorId, BlogUpdateRequestDTO request)
        {
            if (blogId <= 0 || authorId <= 0)
                throw new ArgumentException("ID tác giả hoặc ID blog không hợp lệ");
            var existing = await _repo.GetByIdAsync(blogId);
            if (existing == null)
                throw new KeyNotFoundException($"Blog với ID {blogId} không tồn tại.");
            if (existing.AccId != authorId)
                throw new UnauthorizedAccessException("Bạn chỉ có thể cập nhật blog của bản thân.");
            // Update only provided fields
            if (request.Title != null) existing.Title = request.Title;
            if (request.Content != null) existing.Content = request.Content;
            if (request.IsAnonymous.HasValue) existing.IsAnonymous = request.IsAnonymous;
            if (request.Notes != null) existing.Notes = request.Notes;
            var updated = await _repo.UpdateAsync(blogId, existing);
            return MapToResponseDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid blog ID");

            return await _repo.DeleteAsync(id);
        }

        public async Task<BlogResponseDTO> UpdateBlogStatusAsync(int id, BlogVerificationRequestDTO request)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid blog ID");

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.StaffId <= 0)
                throw new ArgumentException("Invalid staff ID");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Blog with ID {id} not found.");

            // Update verification-related fields only
            existing.BlogStatus = request.BlogStatus;
            existing.StfId = request.StaffId;
            if (request.Notes != null)
                existing.Notes = request.Notes;

            var updated = await _repo.UpdateAsync(id, existing);
            return MapToResponseDto(updated);
        }

        public async Task<BlogResponseDTO> CreateDraftAsync(BlogCreateRequestDTO request)
        {
            ValidateCreateRequest(request);

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var draft = new SocialBlog
            {
                AccId = request.AuthorId,
                Title = request.Title,
                Content = request.Content,
                IsAnonymous = request.IsAnonymous,
                Notes = request.Notes,
                BlogStatus = 1,
                PublishedAt = null
            };

            var created = await _repo.CreateDraftAsync(draft);
            return MapToResponseDto(created);
        }

        public async Task<List<BlogResponseDTO>> GetDraftsByAuthorIdAsync(int authorId)
        {
            if (authorId <= 0)
                throw new ArgumentException("Invalid author ID");

            var drafts = await _repo.GetDraftsByAuthorIdAsync(authorId);
            return drafts.Select(MapToResponseDto).ToList();
        }
    }
}
