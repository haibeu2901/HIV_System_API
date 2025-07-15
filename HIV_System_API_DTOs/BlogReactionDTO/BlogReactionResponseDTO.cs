using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.BlogReactionDTO
{
    public class BlogReactionResponseDTO
    {
        public int ReactionId { get; set; }
        public int BlogId { get; set; }
        public int AccountId { get; set; }
        public bool? ReactionType { get; set; }
        public DateTime ReactedAt { get; set; }
    }
}
