using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class PatientArvMedicationDTO
    {
        public int PamId { get; set; }

        public int ParId { get; set; }

        public int AmdId { get; set; }

        public int? Quantity { get; set; } = 0;
    }
}
