using HIV_System_API_BOs;
using HIV_System_API_DTOs.BlogImageDTO;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
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
            _blogImageRepository = blogImageRepository ?? throw new ArgumentNullException(nameof(blogImageRepository), "Kho lưu trữ hình ảnh blog không được để trống.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Cấu hình không được để trống.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger không được để trống.");

            // Initialize storage path with validation
            var configuredPath = _configuration["ImageStorage:Path"];
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                _logger.LogWarning("ImageStorage:Path is not configured in appsettings.json. Using default path.");
                configuredPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "blog-images");
            }
            _logger.LogInformation("Current Directory: {CurrentDirectory}", Directory.GetCurrentDirectory());
            _storagePath = Path.GetFullPath(configuredPath);
            if (string.IsNullOrWhiteSpace(_storagePath))
            {
                _logger.LogError("Storage path is invalid or null.");
                throw new InvalidOperationException("Không thể khởi tạo đường dẫn lưu trữ. Vui lòng đảm bảo ImageStorage:Path được cấu hình đúng.");
            }

            _baseUrl = _configuration["ImageStorage:BaseUrl"] ?? "/blog-images/";
            if (string.IsNullOrWhiteSpace(_baseUrl))
            {
                _logger.LogWarning("ImageStorage:BaseUrl is not configured in appsettings.json. Using default base URL '/blog-images/'.");
            }

            _logger.LogInformation("Initialized BlogImageService with storage path: {StoragePath} and base URL: {BaseUrl}", _storagePath, _baseUrl);
        }

        public async Task<BlogImageResponseDTO> UploadBlogImageAsync(BlogImageRequestDTO request)
        {
            if (request == null || request.Image == null || request.Image.Length == 0)
            {
                _logger.LogError("Upload request or image is null or empty.");
                throw new ArgumentException("Không có hình ảnh nào được cung cấp.");
            }

            // Size limit check
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (request.Image.Length > maxFileSize)
            {
                _logger.LogError("Image size {ImageSize} bytes exceeds limit of {MaxFileSize} bytes.", request.Image.Length, maxFileSize);
                throw new ArgumentException($"Kích thước hình ảnh vượt quá giới hạn {maxFileSize / (1024 * 1024)}MB.");
            }

            // Ensure storage directory exists
            try
            {
                if (!Directory.Exists(_storagePath))
                {
                    _logger.LogInformation("Creating storage directory: {StoragePath}", _storagePath);
                    Directory.CreateDirectory(_storagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create storage directory: {StoragePath}", _storagePath);
                throw new InvalidOperationException("Không thể tạo thư mục lưu trữ.", ex);
            }

            // Generate unique file name with extension validation
            var fileExtension = Path.GetExtension(request.Image.FileName).ToLower();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (string.IsNullOrWhiteSpace(fileExtension) || !validExtensions.Contains(fileExtension))
            {
                _logger.LogError("Unsupported file extension: {FileExtension}", fileExtension);
                throw new ArgumentException("Chỉ cho phép các tệp JPG, JPEG và PNG.");
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
                _logger.LogInformation("Successfully saved image to {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save image to {FilePath}", filePath);
                throw new InvalidOperationException("Không thể lưu hình ảnh vào ổ đĩa.", ex);
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
                _logger.LogInformation("Successfully saved image metadata to database with ImgId: {ImgId}", savedImage.ImgId);
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
                throw new InvalidOperationException("Không thể lưu siêu dữ liệu hình ảnh.", ex);
            }
        }

        public async Task<BlogImageResponseDTO> GetByIdAsync(int imgId)
        {
            var blogImage = await _blogImageRepository.GetByIdAsync(imgId);
            if (blogImage == null)
            {
                _logger.LogWarning("Image with ID {ImgId} not found.", imgId);
                throw new ArgumentException("Không tìm thấy hình ảnh.");
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
                _logger.LogWarning("Image with ID {ImgId} not found for deletion.", imgId);
                return false;
            }

            var fileName = Path.GetFileName(blogImage.ImageUrl);
            var filePath = Path.Combine(_storagePath, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Successfully deleted image file at {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to delete image file at {FilePath}", filePath);
                    throw new InvalidOperationException("Không thể xóa tệp hình ảnh khỏi ổ đĩa.", ex);
                }
            }
            else
            {
                _logger.LogWarning("Image file not found at {FilePath}, proceeding to delete database record.", filePath);
            }

            try
            {
                await _blogImageRepository.DeleteBlogImageAsync(imgId);
                _logger.LogInformation("Successfully deleted image metadata with ImgId: {ImgId} from database", imgId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image metadata with ImgId: {ImgId} from database", imgId);
                throw new InvalidOperationException("Không thể xóa siêu dữ liệu hình ảnh khỏi cơ sở dữ liệu.", ex);
            }
        }

        public async Task<List<BlogImageResponseDTO>> GetByBlogIdAsync(int blogId)
        {
            try
            {
                var blogImages = await _blogImageRepository.GetByBlogIdAsync(blogId);
                if (blogImages == null || !blogImages.Any())
                {
                    _logger.LogWarning("No images found for SblId: {BlogId}", blogId);
                    throw new ArgumentException("Không tìm thấy hình ảnh cho ID blog được chỉ định.");
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
                _logger.LogInformation("Retrieved {ImageCount} images for SblId: {BlogId}", responseDtos.Count, blogId);
                return responseDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve images for SblId: {BlogId}", blogId);
                throw new InvalidOperationException("Không thể truy xuất hình ảnh.", ex);
            }
        }

        public async Task<List<BlogImageResponseDTO>> UploadImageListAsync(BlogImageListRequestDTO request)
        {
            if (request == null || request.Images == null || !request.Images.Any())
            {
                _logger.LogError("Upload request or images are null or empty.");
                throw new ArgumentException("Không có hình ảnh nào được cung cấp.");
            }

            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            var validExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var responseDtos = new List<BlogImageResponseDTO>();

            // Ensure storage directory exists
            try
            {
                if (!Directory.Exists(_storagePath))
                {
                    _logger.LogInformation("Creating storage directory: {StoragePath}", _storagePath);
                    Directory.CreateDirectory(_storagePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create storage directory: {StoragePath}", _storagePath);
                throw new InvalidOperationException("Không thể tạo thư mục lưu trữ.", ex);
            }

            foreach (var image in request.Images)
            {
                // Size limit check
                if (image.Length > maxFileSize)
                {
                    _logger.LogError("Image size {ImageSize} bytes exceeds limit of {MaxFileSize} bytes.", image.Length, maxFileSize);
                    throw new ArgumentException($"Kích thước hình ảnh '{image.FileName}' vượt quá giới hạn {maxFileSize / (1024 * 1024)}MB.");
                }

                // Validate file extension
                var fileExtension = Path.GetExtension(image.FileName).ToLower();
                if (string.IsNullOrWhiteSpace(fileExtension) || !validExtensions.Contains(fileExtension))
                {
                    _logger.LogError("Unsupported file extension for image '{FileName}': {FileExtension}", image.FileName, fileExtension);
                    throw new ArgumentException($"Hình ảnh '{image.FileName}' có định dạng không được hỗ trợ. Chỉ cho phép các tệp JPG, JPEG và PNG.");
                }

                // Generate unique file name
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_storagePath, fileName);

                // Save file to disk
                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }
                    _logger.LogInformation("Successfully saved image '{FileName}' to {FilePath}", image.FileName, filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save image '{FileName}' to {FilePath}", image.FileName, filePath);
                    throw new InvalidOperationException($"Không thể lưu hình ảnh '{image.FileName}' vào ổ đĩa.", ex);
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
                    _logger.LogInformation("Successfully saved image metadata to database with ImgId: {ImgId}", savedImage.ImgId);
                    responseDtos.Add(new BlogImageResponseDTO
                    {
                        ImgId = savedImage.ImgId,
                        SblId = savedImage.SblId,
                        ImageUrl = savedImage.ImageUrl,
                        UploadedAt = savedImage.UploadedAt
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to save image metadata for '{FileName}' to database.", image.FileName);
                    throw new InvalidOperationException($"Không thể lưu siêu dữ liệu hình ảnh cho '{image.FileName}'.", ex);
                }
            }

            return responseDtos;
        }
    }
}