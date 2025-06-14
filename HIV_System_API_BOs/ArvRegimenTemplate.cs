using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class ArvRegimenTemplate
{
    public int ArtId { get; set; }

    public string? Description { get; set; }

    public byte? Level { get; set; }

    public int? Duration { get; set; }

    public virtual ICollection<ArvMedicationTemplate> ArvMedicationTemplates { get; set; } = new List<ArvMedicationTemplate>();
}
