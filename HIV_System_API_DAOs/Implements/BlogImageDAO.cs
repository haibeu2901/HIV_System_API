using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class BlogImageDAO : IBlogImageDAO
    {
        private readonly HivSystemApiContext _context;
        private static BlogImageDAO _instance;

        public static BlogImageDAO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BlogImageDAO();
                }
                return _instance;
            }
        }
        public BlogImageDAO()
        {
            _context = new HivSystemApiContext();
        }
        public async Task<BlogImage> UploadImageAsync(BlogImage blogImage)
        {
            _context.BlogImages.Add(blogImage);
            await _context.SaveChangesAsync();
            return blogImage;
        }
        public async Task<List<BlogImage>> UploadImageListAsync(List<BlogImage> blogImages)
        {
            _context.BlogImages.AddRange(blogImages);
            await _context.SaveChangesAsync();
            return blogImages;
        }

        public async Task<BlogImage> GetByIdAsync(int imgId)
        {
            return await _context.BlogImages
                .FirstOrDefaultAsync(img => img.ImgId == imgId);
        }
        public async Task<bool> DeleteBlogImageAsync(int imgId)
        {
            var blogImage = await GetByIdAsync(imgId);
            if (blogImage == null)
            {
                return false;
            }
            _context.BlogImages.Remove(blogImage);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<BlogImage>> GetByBlogIdAsync(int blogId)
        {
            return await _context.BlogImages
                .Where(img => img.SblId == blogId)
                .ToListAsync();
        }
    }
}
