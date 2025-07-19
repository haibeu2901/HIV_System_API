using HIV_System_API_DTOs.PatientArvMedicationDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientARVRegimenDTO
{
    public class CreatePatientArvRegimenWithMedicationsRequestDTO
    {
        public PatientArvRegimenRequestDTO Regimen { get; set; } = null!;
        public List<PatientArvMedicationRequestDTO> Medications { get; set; } = null!;
    }

    public class UpdatePatientArvRegimenWithMedicationsRequestDTO
    {
        public PatientArvRegimenRequestDTO RegimenRequest { get; set; } = null!;
        public List<PatientArvMedicationRequestDTO> MedicationRequests { get; set; } = null!;
    }
}
