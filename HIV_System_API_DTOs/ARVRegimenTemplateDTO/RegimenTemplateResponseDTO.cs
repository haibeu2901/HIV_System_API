using HIV_System_API_DTOs.ARVMedicationTemplateDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.ARVRegimenTemplateDTO
{
    public class RegimenTemplateResponseDTO
    {
        public int ArtId { get; set; }
        public string? Description { get; set; }
        public byte? Level { get; set; }
        public int? Duration { get; set; }
        public List<MedicationTemplateResponseDTO>? Medications { get; set; }
    }
}
