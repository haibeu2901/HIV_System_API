using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HIV_System_API_Services.Implements
{
    public class RegimenReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private Timer _reminderTimer;
        private Timer _cancelAppointmentTimer;
        private readonly TimeSpan _dailyExecutionTime = new TimeSpan(0, 0, 0); // Midnight (00:00)

        public RegimenReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RegimenReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Services are starting.");
            ScheduleDailyTasks(stoppingToken);
            return Task.CompletedTask;
        }

        private void ScheduleDailyTasks(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var nextRun = now.AddMinutes(1);



            var timeToNextRun = nextRun - now;
            _logger.LogInformation($"Next tasks scheduled for {nextRun:yyyy-MM-dd HH:mm:ss}.");

            // Timer cho reminder phác đồ
            _reminderTimer = new Timer(
                async state => await ExecuteReminderTaskAsync(stoppingToken),
                null,
                timeToNextRun,
                TimeSpan.FromMinutes(1));

            // Timer cho hủy cuộc hẹn
            _cancelAppointmentTimer = new Timer(
                async state => await ExecuteDailyTasksAsync(stoppingToken),
                null,
                timeToNextRun,
                TimeSpan.FromMinutes(1));
        }

        private async Task ExecuteDailyTasksAsync(CancellationToken stoppingToken)
        {
            try
            {
                // Thực hiện cả hai tác vụ
                await Task.WhenAll(
                    ExecuteReminderTaskAsync(stoppingToken),
                    CancelPastDateAppointmentsAsync(stoppingToken)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing daily tasks.");
            }
            finally
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    ScheduleDailyTasks(stoppingToken);
                }
            }
        }

        private async Task ExecuteReminderTaskAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing regimen reminder task at {Time}.", DateTime.Now);
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var patientArvRegimenService = scope.ServiceProvider.GetRequiredService<IPatientArvRegimenService>();
                    var results = await patientArvRegimenService.SendEndDateReminderNotificationsAsync(7);
                    _logger.LogInformation($"Sent {results.Count} regimen end date reminders.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing regimen reminder task.");
            }
        }

        private async Task CancelPastDateAppointmentsAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing cancel past date appointments task at {Time}.", DateTime.Now);
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
                    await appointmentService.CancelPastDateAppointmentsAsync();
                    _logger.LogInformation("Successfully cancelled past date appointments.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing cancel past date appointments task.");
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("Background Services are disposing.");
            _reminderTimer?.Dispose();
            _cancelAppointmentTimer?.Dispose();
            base.Dispose();
        }
    }
}
