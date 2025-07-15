using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.BlogReactionDTO
{
    public class BlogReactionRequestDTO
    {
        public int BlogId { get; set; }
        public int AccountId { get; set; }
        public bool ReactionType { get; set; } // true for like, false for dislike
    }
}
