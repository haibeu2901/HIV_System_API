using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.ArvMedicationDetailDTO
{
    public class ArvMedicationDetailDTO
    {
        public string? ARVMedicationName { get; set; }
        public string? ARVMedicationDescription { get; set; }
        public string? ARVMedicationDosage { get; set; }
        public double ARVMedicationPrice { get; set; }
        public string? ARVMedicationManufacturer { get; set; }
    }
}
