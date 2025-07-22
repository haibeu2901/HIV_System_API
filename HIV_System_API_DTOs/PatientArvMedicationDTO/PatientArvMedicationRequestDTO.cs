using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientArvMedicationDTO
{
    public class PatientArvMedicationRequestDTO
    {
        public int PatientArvRegId { get; set; }
        public int ArvMedDetailId { get; set; }
        public int? Quantity { get; set; }
        public string? UsageInstructions { get; set; }
    }
}