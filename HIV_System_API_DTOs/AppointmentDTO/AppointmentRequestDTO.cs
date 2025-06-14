using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.Appointment
{
    public class AppointmentRequestDTO
    {
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public DateOnly ApmtDate { get; set; }
        public TimeOnly ApmTime { get; set; }
        public string? Notes { get; set; }
        public byte ApmStatus { get; set; } 
    }
}
