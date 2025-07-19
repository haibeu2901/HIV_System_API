using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class ArvMedicationTemplate
{
    public int AmtId { get; set; }

    public int ArtId { get; set; }

    public int AmdId { get; set; }

    public int? Quantity { get; set; }

    public string? MedicationUsage { get; set; }

    public virtual ArvMedicationDetail Amd { get; set; } = null!;

    public virtual ArvRegimenTemplate Art { get; set; } = null!;
}
