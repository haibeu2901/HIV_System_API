using HIV_System_API_BOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Interfaces
{
    public interface IBlogImageDAO
    {
        Task<BlogImage> UploadImageAsync(BlogImage blogImage);
        Task<List<BlogImage>> UploadImageListAsync(List<BlogImage> blogImages);
        Task<BlogImage> GetByIdAsync(int imgId);
        Task<bool> DeleteBlogImageAsync(int imgId);
        Task<List<BlogImage>> GetByBlogIdAsync(int blogId);
    }
}
