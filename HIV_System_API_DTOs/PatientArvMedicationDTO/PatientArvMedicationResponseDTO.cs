namespace HIV_System_API_DTOs.PatientArvMedicationDTO
{
    public class PatientArvMedicationResponseDTO
    {
        public int PatientArvMedId { get; set; }
        public int PatientArvRegiId { get; set; }
        public int ArvMedId { get; set; }
        public int? Quantity { get; set; }
    }
}