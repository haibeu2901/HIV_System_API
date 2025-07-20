using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.ARVMedicationTemplateDTO
{
    public class MedicationTemplateResponseDTO
    {
        public int ArvMedicationTemplateId { get; set; }
        public int ArvRegimenTemplateId { get; set; }
        public string? ArvRegimenTemplateDescription { get; set; }
        public int ArvMedicationDetailId { get; set; }
        public string? MedicationName { get; set; }
        public string? MedicationDescription { get; set; }
        public string? Dosage { get; set; }
        public string? MedicationType { get; set; }
        public int Quantity { get; set; }
    }
}
