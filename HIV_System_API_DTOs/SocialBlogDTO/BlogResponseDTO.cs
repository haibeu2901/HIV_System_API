using System;
using HIV_System_API_DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIV_System_API_DTOs.BlogReactionDTO;

namespace HIV_System_API_DTOs.SocialBlogDTO
{
    public class BlogResponseDTO
    {
        public int Id { get; set; }
        public int AuthorId { get; set; }
        public int? StaffId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? PublishedDate { get; set; }
        public bool? IsAnonymous { get; set; } = false;
        public string? Notes { get; set; } = string.Empty;
        public byte BlogStatus { get; set; } = 0;
        public List<FullBlogReactionResponseDTO> BlogReaction { get; set; } = new List<FullBlogReactionResponseDTO>();
        public int LikesCount { get; set; } = 0;
        public int DislikesCount { get; set; } = 0;
        public string AuthorName { get; set; } = string.Empty;
        public List<string> ImageUrl { get; set; } = new List<string>();

    }
}
