using HIV_System_API_DTOs.Appointment;
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
    }
}
