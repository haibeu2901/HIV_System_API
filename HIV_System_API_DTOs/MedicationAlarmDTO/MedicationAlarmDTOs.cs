using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DTOs.MedicationAlarmDTO
{
    public class MedicationAlarmRequestDTO
    {
        public int PatientArvMedicationId { get; set; }
        public TimeOnly AlarmTime { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
    }

    public class MedicationAlarmResponseDTO
    {
        public int AlarmId { get; set; }
        public int PatientId { get; set; }
        public int PatientArvMedicationId { get; set; }
        public string? MedicationName { get; set; }
        public string? Dosage { get; set; }
        public string? UsageInstructions { get; set; }
        public TimeOnly AlarmTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Notes { get; set; }
        public DateTime? LastNotificationSent { get; set; }
    }

    public class MedicationAlarmUpdateDTO
    {
        public TimeOnly? AlarmTime { get; set; }
        public bool? IsActive { get; set; }
        public string? Notes { get; set; }
    }
}