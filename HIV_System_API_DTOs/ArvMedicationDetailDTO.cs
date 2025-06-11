using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class ArvMedicationDetailDTO
    {
        public int AmdId { get; set; }
        public string MedName { get; set; } = null!;
        public string? MedDescription { get; set; }
        public string? Dosage { get; set; }
        public double Price { get; set; }
        public string? Manufactorer { get; set; }
    }
}
