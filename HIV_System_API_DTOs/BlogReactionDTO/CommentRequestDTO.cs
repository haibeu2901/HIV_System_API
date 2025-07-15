using System;

namespace HIV_System_API_DTOs.BlogReactionDTO
{
    public class CommentRequestDTO
    {
        public int BlogId { get; set; }
        public int AccountId { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}