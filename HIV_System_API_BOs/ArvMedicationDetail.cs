using System;
using System.Collections.Generic;

namespace HIV_System_API_BOs;

public partial class ArvMedicationDetail
{
    public int AmdId { get; set; }

    public string MedName { get; set; } = null!;

    public string? MedDescription { get; set; }

    public string? Dosage { get; set; }

    public double Price { get; set; }

    public string? Manufactorer { get; set; }

    public virtual ICollection<ArvMedicationTemplate> ArvMedicationTemplates { get; set; } = new List<ArvMedicationTemplate>();

    public virtual ICollection<PatientArvMedication> PatientArvMedications { get; set; } = new List<PatientArvMedication>();
}
