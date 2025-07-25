using HIV_System_API_DTOs.MedicationAlarmDTO;

namespace HIV_System_API_Services.Interfaces
{
    public interface IMedicationAlarmService
    {
        Task<MedicationAlarmResponseDTO> CreateMedicationAlarmAsync(MedicationAlarmRequestDTO request, int patientId);
        Task<List<MedicationAlarmResponseDTO>> GetPersonalMedicationAlarmsAsync(int patientId);
        Task<MedicationAlarmResponseDTO?> GetMedicationAlarmByIdAsync(int alarmId, int patientId);
        Task<MedicationAlarmResponseDTO> UpdateMedicationAlarmAsync(int alarmId, MedicationAlarmUpdateDTO request, int patientId);
        Task<bool> DeleteMedicationAlarmAsync(int alarmId, int patientId);
        Task<bool> ToggleAlarmStatusAsync(int alarmId, bool isActive, int patientId);
        Task ProcessMedicationAlarmsAsync();
    }
}