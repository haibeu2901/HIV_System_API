using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class PatientArvRegimen
{
    public int ParId { get; set; }

    public int PmrId { get; set; }

    public string? Notes { get; set; }

    public byte? RegimenLevel { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public byte? RegimenStatus { get; set; }

    public double? TotalCost { get; set; }

    public virtual ICollection<PatientArvMedication> PatientArvMedications { get; set; } = new List<PatientArvMedication>();

    public virtual PatientMedicalRecord Pmr { get; set; } = null!;
}
