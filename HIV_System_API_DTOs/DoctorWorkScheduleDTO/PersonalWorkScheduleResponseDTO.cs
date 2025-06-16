using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.DoctorWorkScheduleDTO
{
    public class PersonalWorkScheduleResponseDTO
    {
        public byte DayOfWeek { get; set; } // 0: Sunday, 1: Monday, ..., 6: Saturday
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
