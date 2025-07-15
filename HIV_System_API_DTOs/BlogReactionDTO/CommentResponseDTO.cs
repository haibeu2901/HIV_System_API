using System;

namespace HIV_System_API_DTOs.BlogReactionDTO
{
    public class CommentResponseDTO
    {
        public int ReactionId { get; set; }
        public int BlogId { get; set; }
        public int AccountId { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime ReactedAt { get; set; }
    }
}