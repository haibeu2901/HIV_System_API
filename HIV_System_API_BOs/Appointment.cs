using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class Appointment
{
    public int ApmId { get; set; }

    public int PtnId { get; set; }

    public int DctId { get; set; }

    public DateOnly ApmtDate { get; set; }

    public TimeOnly ApmTime { get; set; }

    public string? Notes { get; set; }

    public byte ApmStatus { get; set; }

    public DateOnly? RequestDate { get; set; }

    public TimeOnly? RequestTime { get; set; }

    public int? RequestBy { get; set; }

    public virtual Doctor Dct { get; set; } = null!;

    public virtual Patient Ptn { get; set; } = null!;
}
