namespace HIV_System_API_DTOs.PatientArvMedicationDTO
{
    public class PatientArvMedicationRequestDTO
    {
        public int PatientArvMedId { get; set; }
        public int ArvMedDetailId { get; set; }
        public int? Quantity { get; set; }
    }
}