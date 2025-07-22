using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class PatientArvMedication
{
    public int PamId { get; set; }

    public int ParId { get; set; }

    public int AmdId { get; set; }

    public int? Quantity { get; set; }

    public string? UsageInstructions { get; set; }

    public virtual ArvMedicationDetail Amd { get; set; } = null!;

    public virtual PatientArvRegimen Par { get; set; } = null!;
}
