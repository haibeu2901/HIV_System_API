using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class DoctorPatientMr
{
    public int DpmId { get; set; }

    public int PmrId { get; set; }

    public int DctId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Doctor Dct { get; set; } = null!;

    public virtual PatientMedicalRecord Pmr { get; set; } = null!;
}
