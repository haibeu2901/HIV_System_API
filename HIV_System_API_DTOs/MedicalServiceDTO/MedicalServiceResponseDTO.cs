namespace HIV_System_API_DTOs.MedicalServiceDTO
{
    public class MedicalServiceResponseDTO
    {
        public int ServiceId { get; set; }
        public int AccId { get; set; }
        public string ServiceName { get; set; } = null!;
        public string? ServiceDescription { get; set; }
        public double Price { get; set; }
        public bool? IsAvailable { get; set; }
    }
}