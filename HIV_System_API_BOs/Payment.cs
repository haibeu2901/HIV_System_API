using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_BOs;

public partial class Payment
{
    public int PayId { get; set; }

    public int PmrId { get; set; }

    public int? SrvId { get; set; }

    public string? PaymentIntentId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public byte PaymentStatus { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual PatientMedicalRecord Pmr { get; set; } = null!;

    public virtual MedicalService? Srv { get; set; }
}

