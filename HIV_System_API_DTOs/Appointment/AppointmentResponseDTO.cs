using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.Appointment
{
    public class AppointmentResponseDTO
    {
        public int ApmId { get; set; }
        public int PmrId { get; set; }
        public string? PatientName { get; set; }
        public int DctId { get; set; }
        public string? DoctorName { get; set; }
        public DateOnly ApmtDate { get; set; }
        public TimeOnly ApmTime { get; set; }
        public string? Notes { get; set; }
        public byte ApmStatus { get; set; }
    }
}
