using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class TestResult
{
    public int TrsId { get; set; }

    public int PmrId { get; set; }

    public DateOnly TestDate { get; set; }

    public bool? ResultValue { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<ComponentTestResult> ComponentTestResults { get; set; } = new List<ComponentTestResult>();

    public virtual PatientMedicalRecord Pmr { get; set; } = null!;
}
