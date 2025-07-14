namespace HIV_System_API_DTOs.SocialBlogDTO
{
    public class BlogDraftRequestDTO
    {
        public int AuthorId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsAnonymous { get; set; }
        public string? Notes { get; set; }
    }
}