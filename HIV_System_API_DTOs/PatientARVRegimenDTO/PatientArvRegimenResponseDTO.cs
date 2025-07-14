using HIV_System_API_DTOs.PatientArvMedicationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientARVRegimenDTO
{
    public class PatientArvRegimenResponseDTO
    {
        public int PatientArvRegiId { get; set; }

        public int PatientMedRecordId { get; set; }

        public string? Notes { get; set; }

        public byte? RegimenLevel { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public byte? RegimenStatus { get; set; }

        public double? TotalCost { get; set; }

        public List<PatientArvMedicationResponseDTO> ARVMedications { get; set; } = new List<PatientArvMedicationResponseDTO>();
    }
}
