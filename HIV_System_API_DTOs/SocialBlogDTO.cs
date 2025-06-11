using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class SocialBlogDTO
    {
        public int SblId { get; set; }

        public int AccId { get; set; }

        public int? StfId { get; set; }

        public string Title { get; set; } = null!;

        public string? Content { get; set; }
        
        public bool? IsAnonymous { get; set; }

        public string? Notes { get; set; }

        public byte BlogStatus { get; set; }

        public DateTime? PublishedAt { get; set; }
    }
}
