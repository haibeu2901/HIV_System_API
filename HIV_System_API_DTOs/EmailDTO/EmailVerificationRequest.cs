namespace HIV_System_API_DTOs.EmailDTO
{
    public class EmailVerificationRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}