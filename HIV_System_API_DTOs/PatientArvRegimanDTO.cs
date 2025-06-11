using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs
{
    public class PatientArvRegimanDTO
    {
        public int ParId { get; set; }

        public int PmrId { get; set; }

        public string? Notes { get; set; }

        public byte? RegimenLevel { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public byte? RegimenStatus { get; set; }

        public double? TotalCost { get; set; } = 0;
    }
}
