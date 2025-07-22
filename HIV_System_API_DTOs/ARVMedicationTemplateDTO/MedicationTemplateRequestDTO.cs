using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.ARVMedicationTemplateDTO
{
    public class MedicationTemplateRequestDTO
    {
        public int ArtId { get; set; }
        public int AmdId { get; set; }
        public int? Quantity { get; set; }
        public string? MedicationUsage { get; set; }
    }
}
