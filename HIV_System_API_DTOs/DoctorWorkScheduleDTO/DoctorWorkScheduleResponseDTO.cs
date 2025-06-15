using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.DoctorWorkScheduleDTO
{
    public class DoctorWorkScheduleResponseDTO
    {
        public int DocWorkScheduleId { get; set; }
        public int DoctorId { get; set; }
        public int DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
