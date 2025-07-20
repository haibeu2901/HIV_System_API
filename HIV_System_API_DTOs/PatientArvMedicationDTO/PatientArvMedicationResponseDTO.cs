using HIV_System_API_DTOs.ArvMedicationDetailDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientArvMedicationDTO
{
    public class PatientArvMedicationResponseDTO
    {
        public int PatientArvMedId { get; set; }
        public int PatientArvRegiId { get; set; }
        public int ArvMedId { get; set; }
        public int? Quantity { get; set; }
        public string? UsageInstructions { get; set; }
        public ArvMedicationDetailResponseDTO? MedicationDetail { get; set; }
    }
}