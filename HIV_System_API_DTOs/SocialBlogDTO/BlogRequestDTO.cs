using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.SocialBlogDTO
{
    public class BlogRequestDTO
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string AuthorId { get; set; }
        public DateTime PublishedDate { get; set; }
        public bool IsAnonymous { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public byte BlogStatus { get; set; } = 0;
    }
}
