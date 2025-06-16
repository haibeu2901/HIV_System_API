namespace HIV_System_API_DTOs.AccountDTO
{
    public class PendingPatientRegistrationDTO
    {
        public string AccUsername { get; set; } = string.Empty;
        public string AccPassword { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Fullname { get; set; }
        public DateTime? Dob { get; set; }
        public bool? Gender { get; set; }
    }
}