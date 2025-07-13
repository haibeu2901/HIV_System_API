namespace HIV_System_API_DTOs.SocialBlogDTO
{
    public class BlogVerificationRequestDTO
    {
        public byte BlogStatus { get; set; }
        public int StaffId { get; set; }
        public string? Notes { get; set; }
    }
}