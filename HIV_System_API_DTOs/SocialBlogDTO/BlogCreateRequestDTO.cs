namespace HIV_System_API_DTOs.SocialBlogDTO
{
    public class BlogCreateRequestDTO
    {
        public int AuthorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public bool? IsAnonymous { get; set; } = false;
        public string? Notes { get; set; } = string.Empty;
    }
}