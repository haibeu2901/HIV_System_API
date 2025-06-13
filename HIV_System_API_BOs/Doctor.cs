using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Doctor
{
    public int DctId { get; set; }

    public int AccId { get; set; }

    public string? Degree { get; set; }

    public string? Bio { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorWorkSchedule> DoctorWorkSchedules { get; set; } = new List<DoctorWorkSchedule>();
}
