using HIV_System_API_BOs;
using HIV_System_API_DTOs.MedicationAlarmDTO;
using HIV_System_API_DTOs.NotificationDTO;
using HIV_System_API_Repositories.Implements;
using HIV_System_API_Repositories.Interfaces;
using HIV_System_API_Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace HIV_System_API_Services.Implements
{
    public class MedicationAlarmService : IMedicationAlarmService
    {
        private readonly INotificationService _notificationService;
        private readonly IPatientArvMedicationRepo _patientArvMedicationRepo;
        private readonly HivSystemApiContext _context;
        
        // In-memory storage for medication alarms (thread-safe)
        private static readonly ConcurrentDictionary<int, MedicationAlarmData> _medicationAlarms = new();
        private static int _alarmIdCounter = 1;

        public MedicationAlarmService()
        {
            _notificationService = new NotificationService();
            _patientArvMedicationRepo = new PatientArvMedicationRepo();
            _context = new HivSystemApiContext();
        }

        public MedicationAlarmService(
            INotificationService notificationService,
            IPatientArvMedicationRepo patientArvMedicationRepo,
            HivSystemApiContext context)
        {
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _patientArvMedicationRepo = patientArvMedicationRepo ?? throw new ArgumentNullException(nameof(patientArvMedicationRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task<bool> ValidatePatientArvRegimenStatusAsync(int patientId)
        {
            // Check if patient has an active ARV regimen (status = 2)
            var activeRegimen = await _context.PatientArvRegimen
                .Include(par => par.Pmr)
                .Where(par => par.Pmr.PtnId == patientId && par.RegimenStatus == 2)
                .FirstOrDefaultAsync();

            return activeRegimen != null;
        }

        private async Task<bool> ValidatePatientArvMedicationRegimenStatusAsync(int patientArvMedicationId)
        {
            // Check if the medication belongs to an active ARV regimen (status = 2)
            var medication = await _context.PatientArvMedications
                .Include(pam => pam.Par)
                .FirstOrDefaultAsync(pam => pam.PamId == patientArvMedicationId);

            return medication?.Par?.RegimenStatus == 2;
        }

        public async Task<MedicationAlarmResponseDTO> CreateMedicationAlarmAsync(MedicationAlarmRequestDTO request, int patientId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ", nameof(patientId));

            // Validate patient exists
            var patient = await _context.Patients
                .Include(p => p.Acc)
                .FirstOrDefaultAsync(p => p.PtnId == patientId);
            if (patient == null)
                throw new InvalidOperationException($"Bệnh nhân với ID {patientId} không tồn tại.");

            // ADDED: Validate patient has active ARV regimen
            if (!await ValidatePatientArvRegimenStatusAsync(patientId))
                throw new InvalidOperationException("Không thể tạo nhắc nhở uống thuốc. Bệnh nhân không có phác đồ ARV đang hoạt động (trạng thái = 2).");

            // Validate medication exists and belongs to patient
            var medication = await _context.PatientArvMedications
                .Include(pam => pam.Amd)
                .Include(pam => pam.Par)
                    .ThenInclude(par => par.Pmr)
                .FirstOrDefaultAsync(pam => pam.PamId == request.PatientArvMedicationId);

            if (medication == null)
                throw new InvalidOperationException($"Thuốc với ID {request.PatientArvMedicationId} không tồn tại.");

            if (medication.Par.Pmr.PtnId != patientId)
                throw new UnauthorizedAccessException("Bạn không có quyền tạo báo động cho thuốc này.");

            // ADDED: Validate medication belongs to active ARV regimen
            if (!await ValidatePatientArvMedicationRegimenStatusAsync(request.PatientArvMedicationId))
                throw new InvalidOperationException("Không thể tạo nhắc nhở uống thuốc. Thuốc này không thuộc phác đồ ARV đang hoạt động (trạng thái = 2).");

            // Check if alarm already exists for this medication
            var existingAlarm = _medicationAlarms.Values
                .FirstOrDefault(a => a.PatientArvMedicationId == request.PatientArvMedicationId && a.PatientId == patientId);
            if (existingAlarm != null)
                throw new InvalidOperationException("Đã có báo động cho thuốc này. Vui lòng cập nhật báo động hiện có.");

            // Create new alarm
            var alarmId = Interlocked.Increment(ref _alarmIdCounter);
            var alarmData = new MedicationAlarmData
            {
                AlarmId = alarmId,
                PatientId = patientId,
                PatientAccountId = patient.AccId,
                PatientArvMedicationId = request.PatientArvMedicationId,
                MedicationName = medication.Amd?.MedName ?? "Unknown",
                Dosage = medication.Amd?.Dosage ?? "",
                UsageInstructions = medication.UsageInstructions ?? "",
                AlarmTime = request.AlarmTime,
                IsActive = request.IsActive,
                Notes = request.Notes,
                CreatedAt = DateTime.Now
            };

            _medicationAlarms.TryAdd(alarmId, alarmData);

            return MapToResponseDTO(alarmData);
        }

        public async Task<List<MedicationAlarmResponseDTO>> GetPersonalMedicationAlarmsAsync(int patientId)
        {
            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ", nameof(patientId));

            // ADDED: Filter alarms based on active ARV regimen status
            var patientAlarms = new List<MedicationAlarmResponseDTO>();
            
            foreach (var alarm in _medicationAlarms.Values.Where(a => a.PatientId == patientId))
            {
                // Check if the medication still belongs to an active regimen
                if (await ValidatePatientArvMedicationRegimenStatusAsync(alarm.PatientArvMedicationId))
                {
                    patientAlarms.Add(MapToResponseDTO(alarm));
                }
                else
                {
                    // Optionally, you can automatically deactivate alarms for inactive regimens
                    alarm.IsActive = false;
                    alarm.UpdatedAt = DateTime.Now;
                    patientAlarms.Add(MapToResponseDTO(alarm)); // Still return it but marked as inactive
                }
            }

            return patientAlarms.OrderBy(a => a.AlarmTime).ToList();
        }

        public async Task<MedicationAlarmResponseDTO?> GetMedicationAlarmByIdAsync(int alarmId, int patientId)
        {
            if (alarmId <= 0)
                throw new ArgumentException("ID báo động không hợp lệ", nameof(alarmId));

            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ", nameof(patientId));

            if (_medicationAlarms.TryGetValue(alarmId, out var alarm) && alarm.PatientId == patientId)
            {
                // ADDED: Check if medication still belongs to active regimen
                if (!await ValidatePatientArvMedicationRegimenStatusAsync(alarm.PatientArvMedicationId))
                {
                    // Automatically deactivate alarm for inactive regimen
                    alarm.IsActive = false;
                    alarm.UpdatedAt = DateTime.Now;
                }

                return MapToResponseDTO(alarm);
            }

            return null;
        }

        public async Task<MedicationAlarmResponseDTO> UpdateMedicationAlarmAsync(int alarmId, MedicationAlarmUpdateDTO request, int patientId)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (alarmId <= 0)
                throw new ArgumentException("ID báo động không hợp lệ", nameof(alarmId));

            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ", nameof(patientId));

            if (!_medicationAlarms.TryGetValue(alarmId, out var alarm))
                throw new InvalidOperationException($"Báo động với ID {alarmId} không tồn tại.");

            if (alarm.PatientId != patientId)
                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật báo động này.");

            // ADDED: Validate medication still belongs to active regimen before allowing updates
            if (!await ValidatePatientArvMedicationRegimenStatusAsync(alarm.PatientArvMedicationId))
                throw new InvalidOperationException("Không thể cập nhật nhắc nhở uống thuốc. Thuốc này không thuộc phác đồ ARV đang hoạt động (trạng thái = 2).");

            // Update fields
            if (request.AlarmTime.HasValue)
                alarm.AlarmTime = request.AlarmTime.Value;
            if (request.IsActive.HasValue)
                alarm.IsActive = request.IsActive.Value;
            if (request.Notes != null)
                alarm.Notes = request.Notes;

            alarm.UpdatedAt = DateTime.Now;

            return MapToResponseDTO(alarm);
        }

        public async Task<bool> DeleteMedicationAlarmAsync(int alarmId, int patientId)
        {
            if (alarmId <= 0)
                throw new ArgumentException("ID báo động không hợp lệ", nameof(alarmId));

            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ", nameof(patientId));

            if (!_medicationAlarms.TryGetValue(alarmId, out var alarm))
                return false;

            if (alarm.PatientId != patientId)
                throw new UnauthorizedAccessException("Bạn không có quyền xóa báo động này.");

            // Note: We allow deletion regardless of regimen status
            // This allows cleanup of old alarms from inactive regimens
            return _medicationAlarms.TryRemove(alarmId, out _);
        }

        public async Task<bool> ToggleAlarmStatusAsync(int alarmId, bool isActive, int patientId)
        {
            if (alarmId <= 0)
                throw new ArgumentException("ID báo động không hợp lệ", nameof(alarmId));

            if (patientId <= 0)
                throw new ArgumentException("ID bệnh nhân không hợp lệ", nameof(patientId));

            if (!_medicationAlarms.TryGetValue(alarmId, out var alarm))
                return false;

            if (alarm.PatientId != patientId)
                throw new UnauthorizedAccessException("Bạn không có quyền thay đổi báo động này.");

            // ADDED: Only allow activation if regimen is active
            if (isActive && !await ValidatePatientArvMedicationRegimenStatusAsync(alarm.PatientArvMedicationId))
                throw new InvalidOperationException("Không thể kích hoạt nhắc nhở uống thuốc. Thuốc này không thuộc phác đồ ARV đang hoạt động (trạng thái = 2).");

            alarm.IsActive = isActive;
            alarm.UpdatedAt = DateTime.Now;

            return true;
        }

        public async Task ProcessMedicationAlarmsAsync()
        {
            var currentTime = DateTime.Now;
            var currentTimeOnly = TimeOnly.FromDateTime(currentTime);
            var today = currentTime.Date;

            var dueAlarms = _medicationAlarms.Values
                .Where(a => a.IsActive && 
                           Math.Abs((a.AlarmTime.ToTimeSpan() - currentTimeOnly.ToTimeSpan()).TotalMinutes) <= 5 &&
                           (a.LastNotificationSent == null || a.LastNotificationSent.Value.Date < today))
                .ToList();

            foreach (var alarm in dueAlarms)
            {
                try
                {
                    // ADDED: Double-check regimen status before sending notification
                    if (await ValidatePatientArvMedicationRegimenStatusAsync(alarm.PatientArvMedicationId))
                    {
                        await SendMedicationAlarmNotificationAsync(alarm);
                        alarm.LastNotificationSent = currentTime;
                    }
                    else
                    {
                        // Automatically deactivate alarm if regimen is no longer active
                        alarm.IsActive = false;
                        alarm.UpdatedAt = currentTime;
                        Console.WriteLine($"Alarm {alarm.AlarmId} deactivated due to inactive ARV regimen");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue processing other alarms
                    Console.WriteLine($"Error sending alarm notification for AlarmId {alarm.AlarmId}: {ex.Message}");
                }
            }
        }

        private async Task SendMedicationAlarmNotificationAsync(MedicationAlarmData alarm)
        {
            var message = $"⏰ Đã đến giờ uống thuốc: {alarm.MedicationName}";
            if (!string.IsNullOrEmpty(alarm.Dosage))
                message += $" ({alarm.Dosage})";
            if (!string.IsNullOrEmpty(alarm.UsageInstructions))
                message += $". Hướng dẫn: {alarm.UsageInstructions}";
            if (!string.IsNullOrEmpty(alarm.Notes))
                message += $". Ghi chú: {alarm.Notes}";

            var notificationDto = new CreateNotificationRequestDTO
            {
                NotiType = "Nhắc nhở uống thuốc",
                NotiMessage = message,
                SendAt = DateTime.Now
            };

            await _notificationService.CreateAndSendToAccountIdAsync(notificationDto, alarm.PatientAccountId);
        }

        private static MedicationAlarmResponseDTO MapToResponseDTO(MedicationAlarmData data)
        {
            return new MedicationAlarmResponseDTO
            {
                AlarmId = data.AlarmId,
                PatientId = data.PatientId,
                PatientArvMedicationId = data.PatientArvMedicationId,
                MedicationName = data.MedicationName,
                Dosage = data.Dosage,
                UsageInstructions = data.UsageInstructions,
                AlarmTime = data.AlarmTime,
                IsActive = data.IsActive,
                CreatedAt = data.CreatedAt,
                UpdatedAt = data.UpdatedAt,
                Notes = data.Notes,
                LastNotificationSent = data.LastNotificationSent
            };
        }

        // Internal data structure for in-memory storage
        private class MedicationAlarmData
        {
            public int AlarmId { get; set; }
            public int PatientId { get; set; }
            public int PatientAccountId { get; set; }
            public int PatientArvMedicationId { get; set; }
            public string MedicationName { get; set; } = string.Empty;
            public string Dosage { get; set; } = string.Empty;
            public string UsageInstructions { get; set; } = string.Empty;
            public TimeOnly AlarmTime { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string? Notes { get; set; }
            public DateTime? LastNotificationSent { get; set; }
        }
    }
}