using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.DoctorWorkScheduleDTO
{
    public class PersonalWorkScheduleResponseDTO
    {
        public byte DayOfWeek { get; set; } // 1: Sunday, 2: Monday, ..., 7: Saturday
        public DateOnly WorkDate { get; set; }
        public bool IsAvailable { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
