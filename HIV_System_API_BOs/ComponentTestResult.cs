using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class ComponentTestResult
{
    public int CtrId { get; set; }

    public int TrsId { get; set; }

    public int StfId { get; set; }

    public string CtrName { get; set; } = null!;

    public string? CtrDescription { get; set; }

    public string? ResultValue { get; set; }

    public string? Notes { get; set; }

    public virtual Staff Stf { get; set; } = null!;

    public virtual TestResult Trs { get; set; } = null!;
}
