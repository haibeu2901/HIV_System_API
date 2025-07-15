using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.BlogImageDTO
{
    public class BlogImageListRequestDTO
    {
        public int SblId { get; set; }
        public List<IFormFile> Images { get; set; }
    }
}
