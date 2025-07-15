namespace HIV_System_API_DTOs.SocialBlogDTO
{
    public class BlogUpdateRequestDTO
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsAnonymous { get; set; }
        public string? Notes { get; set; }
        public byte? BlogStatus { get; set; }
        public int? StaffId { get; set; }
    }
}