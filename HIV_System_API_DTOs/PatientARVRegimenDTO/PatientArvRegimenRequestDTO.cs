using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientARVRegimenDTO
{
    public class PatientArvRegimenRequestDTO
    {
        // Remove ParId since it should be auto-generated
        public int PmrId { get; set; }  // Required
        public string? Notes { get; set; }
        public byte? RegimenLevel { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public byte? RegimenStatus { get; set; }
        public double? TotalCost { get; set; }
    }
}
