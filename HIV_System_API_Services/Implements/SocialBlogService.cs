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

namespace HIV_System_API_Services.Implements
{
    public class SocialBlogService : ISocialBlogService
    {
        private readonly ISocialBlogRepo _repo;
        private readonly INotificationRepo _notificationRepo;

        public SocialBlogService()
        {
            _repo = new SocialBlogRepo();
            _notificationRepo = new NotificationRepo();
        }

        public SocialBlogService(ISocialBlogRepo repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public SocialBlogService(ISocialBlogRepo repo, INotificationRepo notificationService)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _notificationRepo = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        }

        // --- Mapping and Validation ---

        private static SocialBlog MapToEntity(BlogCreateRequestDTO dto)
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

        private static BlogResponseDTO MapToResponseDto(SocialBlog blog)
        {
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
                BlogStatus = blog.BlogStatus
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
                throw new ArgumentException("Invalid blog ID", nameof(id));

            var blog = await _repo.GetByIdAsync(id);
            return blog == null ? null : MapToResponseDto(blog);
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
                throw new ArgumentException("Invalid blog ID", nameof(id));

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

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid blog ID", nameof(id));

            return await _repo.DeleteAsync(id);
        }

        public async Task<BlogResponseDTO> UpdateBlogStatusAsync(int id, BlogVerificationRequestDTO request)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid blog ID", nameof(id));

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
            if (request.AuthorId <= 0)
                throw new ArgumentException("AuthorId is required and must be positive.");

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
                throw new ArgumentException("Invalid author ID", nameof(authorId));

            var drafts = await _repo.GetDraftsByAuthorIdAsync(authorId);
            return drafts.Select(MapToResponseDto).ToList();
        }
    }
}