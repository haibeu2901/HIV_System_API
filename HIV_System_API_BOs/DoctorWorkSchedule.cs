using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class DoctorWorkSchedule
{
    public int DwsId { get; set; }

    public int DoctorId { get; set; }

    public byte? DayOfWeek { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;
}
