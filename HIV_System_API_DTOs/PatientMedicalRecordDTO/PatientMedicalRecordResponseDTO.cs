using HIV_System_API_DTOs.Appointment;
using HIV_System_API_DTOs.PatientARVRegimenDTO;
using HIV_System_API_DTOs.PaymentDTO;
using HIV_System_API_DTOs.TestResultDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.PatientMedicalRecordDTO
{
    public class PatientMedicalRecordResponseDTO
    {
        public int PmrId { get; set; }
        public int PtnId { get; set; }
        public List<AppointmentResponseDTO> Appointments { get; set; } = new List<AppointmentResponseDTO>();
        public List<TestResultResponseDTO> TestResults { get; set; } = new List<TestResultResponseDTO>();
        public List<PatientArvRegimenResponseDTO> ARVRegimens { get; set; } = new List<PatientArvRegimenResponseDTO>();
        public List<PaymentResponseDTO> Payments { get; set; } = new List<PaymentResponseDTO>();
    }
}
