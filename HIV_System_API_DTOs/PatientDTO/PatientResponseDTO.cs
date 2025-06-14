using HIV_System_API_DTOs.AccountDTO;
using HIV_System_API_DTOs.PatientMedicalRecordDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientDTO
{
    public class PatientResponseDTO
    {
        public int PtnId { get; set; }
        public int AccId { get; set; }
        public AccountResponseDTO? Account { get; set; }
        public PatientMedicalRecordResponseDTO? MedicalRecord { get; set; }
    }
}
