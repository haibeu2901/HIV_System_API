using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class MedicalService
{
    public int SrvId { get; set; }

    public int AccId { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? ServiceDescription { get; set; }

    public double Price { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Account Acc { get; set; } = null!;
}
