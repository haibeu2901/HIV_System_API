using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Doctor
{
    public int DctId { get; set; }

    public int AccId { get; set; }

    public string? Degree { get; set; }

    public string? Bio { get; set; }

    public virtual Account Acc { get; set; } = null!;

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorPatientMr> DoctorPatientMrs { get; set; } = new List<DoctorPatientMr>();
}
