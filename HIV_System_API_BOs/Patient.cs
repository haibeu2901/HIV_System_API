using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Patient
{
    public int PtnId { get; set; }

    public int AccId { get; set; }

    public virtual Account Acc { get; set; } = null!;

    public virtual PatientMedicalRecord? PatientMedicalRecord { get; set; }
}
