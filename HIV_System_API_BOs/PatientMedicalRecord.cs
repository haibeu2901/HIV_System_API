using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class PatientMedicalRecord
{
    public int PmrId { get; set; }

    public int PtnId { get; set; }

    public virtual ICollection<DoctorPatientMr> DoctorPatientMrs { get; set; } = new List<DoctorPatientMr>();

    public virtual ICollection<PatientArvRegiman> PatientArvRegimen { get; set; } = new List<PatientArvRegiman>();

    public virtual Patient Ptn { get; set; } = null!;

    public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
}
