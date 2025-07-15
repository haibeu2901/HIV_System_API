using HIV_System_API_BOs;
using HIV_System_API_DTOs.BlogImageDTO;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class BlogImageService : IBlogImageService
    {
        private readonly IBlogImageRepo _blogImageRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlogImageService> _logger;
        private readonly string _storagePath;
        private readonly string _baseUrl;

        public BlogImageService(IBlogImageRepo blogImageRepository, IConfiguration configuration, ILogger<BlogImageService> logger)
        {
            _blogImageRepository = blogImageRepository ?? throw new ArgumentNullException(nameof(blogImageRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialize storage path with validation
            var configuredPath = _configuration["ImageStorage:Path"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                _logger.LogWarning("ImageStorage:Path is not configured in appsettings.json. Using default path.");
                configuredPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "blog-images");
            }
            _logger.LogInformation($"Current Directory: {Directory.GetCurrentDirectory()}"); // Debug log
            _storagePath = Path.GetFullPath(configuredPath);
            if (string.IsNullOrWhiteSpace(_storagePath))
            {
                _logger.LogError("Storage path is invalid or null.");
                throw new InvalidOperationException("Failed to initialize storage path. Ensure ImageStorage:Path is configured correctly.");
            }

            _baseUrl = _configuration["ImageStorage:BaseUrl"] ?? "/blog-images/";
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                _logger.LogWarning("ImageStorage:BaseUrl is not configured in appsettings.json. Using default base URL '/blog-images/'.");
            }

            _logger.LogInformation($"Initialized BlogImageService with storage path: {_storagePath} and base URL: {_baseUrl}");
        }

        public async Task<BlogImageResponseDTO> UploadBlogImageAsync(BlogImageRequestDTO request)
        {
            if (request == null || request.Image == null || request.Image.Length == 0)
            {
                _logger.LogError("Upload request or image is null or empty.");
                throw new ArgumentException("No image provided");
            }

            // Size limit check
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (request.Image.Length > maxFileSize)
            {
                _logger.LogError($"Image size {request.Image.Length} bytes exceeds limit of {maxFileSize} bytes.");
                throw new ArgumentException($"Image size exceeds {maxFileSize / (1024 * 1024)}MB limit.");
            }

            // Ensure storage directory exists
            try
            {
                if (!Directory.Exists(_storagePath))
                {
                    _logger.LogInformation($"Creating storage directory: {_storagePath}");
                    Directory.CreateDirectory(_storagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create storage directory: {_storagePath}");
                throw new InvalidOperationException("Failed to create storage directory.", ex);
            }

            // Generate unique file name with extension validation
            var fileExtension = Path.GetExtension(request.Image.FileName).ToLower();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (string.IsNullOrWhiteSpace(fileExtension) || !validExtensions.Contains(fileExtension))
            {
                _logger.LogError($"Unsupported file extension: {fileExtension}");
                throw new ArgumentException("Only JPG, JPEG, and PNG files are allowed.");
            }

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_storagePath, fileName);

            // Save file to disk
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.Image.CopyToAsync(stream);
                }
                _logger.LogInformation($"Successfully saved image to {filePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to save image to {filePath}");
                throw new InvalidOperationException("Failed to save image to disk.", ex);
            }

            // Create BlogImage entity
            var blogImage = new BlogImage
            {
                SblId = request.SblId,
                ImageUrl = $"{_baseUrl}{fileName}",
                UploadedAt = DateTime.UtcNow
            };

            // Save to database
            try
            {
                var savedImage = await _blogImageRepository.UploadImageAsync(blogImage);
                _logger.LogInformation($"Successfully saved image metadata to database with ImgId: {savedImage.ImgId}");
                return new BlogImageResponseDTO
                {
                    ImgId = savedImage.ImgId,
                    SblId = savedImage.SblId,
                    ImageUrl = savedImage.ImageUrl,
                    UploadedAt = savedImage.UploadedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image metadata to database.");
                throw new InvalidOperationException("Failed to save image metadata.", ex);
            }
        }

        public async Task<BlogImageResponseDTO> GetByIdAsync(int imgId)
        {
            var blogImage = await _blogImageRepository.GetByIdAsync(imgId);
            if (blogImage == null)
            {
                _logger.LogWarning($"Image with ID {imgId} not found.");
                throw new ArgumentException("Image not found");
            }

            return new BlogImageResponseDTO
            {
                ImgId = blogImage.ImgId,
                SblId = blogImage.SblId,
                ImageUrl = blogImage.ImageUrl,
                UploadedAt = blogImage.UploadedAt
            };
        }
        public async Task<bool> DeleteBlogImageAsync(int imgId)
        {
            var blogImage = await _blogImageRepository.GetByIdAsync(imgId);
            if (blogImage == null)
            {
                _logger.LogWarning($"Image with ID {imgId} not found for deletion.");
                return false; // Return false if image not found
            }

            var fileName = Path.GetFileName(blogImage.ImageUrl);
            var filePath = Path.Combine(_storagePath, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation($"Successfully deleted image file at {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to delete image file at {filePath}");
                    throw new InvalidOperationException("Failed to delete image file from disk.", ex);
                }
            }
            else
            {
                _logger.LogWarning($"Image file not found at {filePath}, proceeding to delete database record.");
            }

            try
            {
                await _blogImageRepository.DeleteBlogImageAsync(imgId);
                _logger.LogInformation($"Successfully deleted image metadata with ImgId: {imgId} from database");
                return true; // Return true on successful deletion
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to delete image metadata with ImgId: {imgId} from database");
                throw new InvalidOperationException("Failed to delete image metadata from database.", ex);
            }
        }
        public async Task<List<BlogImageResponseDTO>> GetByBlogIdAsync(int sblId)
        {
            try
            {
                var blogImages = await _blogImageRepository.GetByBlogIdAsync(sblId);
                if (blogImages == null || !blogImages.Any())
                {
                    _logger.LogWarning($"No images found for SblId: {sblId}");
                    throw new ArgumentException("No images found for the specified blog ID");
                }

                var responseDtos = new List<BlogImageResponseDTO>();
                foreach (var blogImage in blogImages)
                {
                    responseDtos.Add(new BlogImageResponseDTO
                    {
                        ImgId = blogImage.ImgId,
                        SblId = blogImage.SblId,
                        ImageUrl = blogImage.ImageUrl,
                        UploadedAt = blogImage.UploadedAt
                    });
                }
                _logger.LogInformation($"Retrieved {responseDtos.Count} images for SblId: {sblId}");
                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to retrieve images for SblId: {sblId}");
                throw new InvalidOperationException("Failed to retrieve images.", ex);
            }
        }
    }
}