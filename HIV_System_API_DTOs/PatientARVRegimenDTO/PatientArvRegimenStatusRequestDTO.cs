using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientARVRegimenDTO
{
    public class PatientArvRegimenStatusRequestDTO
    {
        public string Notes { get; set; } // Optional notes about the regimen status
        public byte? RegimenStatus { get; set; }

    }
}
