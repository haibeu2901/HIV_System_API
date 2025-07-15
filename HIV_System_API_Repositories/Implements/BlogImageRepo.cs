using HIV_System_API_BOs;
using HIV_System_API_DAOs.Implements;
using HIV_System_API_DAOs.Interfaces;
using HIV_System_API_Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_Repositories.Implements
{
    public class BlogImageRepo : IBlogImageRepo
    {
        public async Task<BlogImage> UploadImageAsync(BlogImage blogImage)
        {
            return await BlogImageDAO.Instance.UploadImageAsync(blogImage);
        }

        public async Task<BlogImage> GetByIdAsync(int imgId)
        {
            return await BlogImageDAO.Instance.GetByIdAsync(imgId);
        }
        public async Task<bool> DeleteBlogImageAsync(int imgId)
        {
            return await BlogImageDAO.Instance.DeleteBlogImageAsync(imgId);
        }
        public async Task<List<BlogImage>> GetByBlogIdAsync(int blogId)
        {
            return await BlogImageDAO.Instance.GetByBlogIdAsync(blogId);
        }
    }
}
