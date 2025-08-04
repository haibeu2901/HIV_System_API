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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private Timer _dailyTimer;
        private readonly TimeSpan _dailyExecutionTime = new TimeSpan(0, 0, 0); // Midnight (00:00)

        public RegimenReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RegimenReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
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
            _dailyTimer = new Timer(
                async state => await ExecuteDailyTasksAsync(stoppingToken),
                null,
                timeToNextRun,
                TimeSpan.FromMinutes(1));
        }

        private async Task ExecuteDailyTasksAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var patientArvRegimenService = scope.ServiceProvider.GetRequiredService<IPatientArvRegimenService>();
                var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();

                // Execute tasks sequentially
                await ArvRegimenReminderAsync(patientArvRegimenService, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); // Add small delay between tasks
                await CancelPastDateAppointmentsAsync(appointmentService, stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                await AppontmentReminderAsync(appointmentService, stoppingToken);
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

        private async Task ArvRegimenReminderAsync(IPatientArvRegimenService service, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing regimen reminder task at {Time}.", DateTime.Now);
            try
            {
                var results = await service.SendEndDateReminderNotificationsAsync(7);
                _logger.LogInformation($"Sent {results.Count} regimen end date reminders.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing regimen reminder task.");
            }
        }

        private async Task CancelPastDateAppointmentsAsync(IAppointmentService service, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing cancel past date appointments task at {Time}.", DateTime.Now);
            try
            {
                await service.CancelPastDateAppointmentsAsync();
                _logger.LogInformation("Successfully cancelled all past date appointments.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing cancel past date appointments task.");
            }
        }

        private async Task AppontmentReminderAsync(IAppointmentService service, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing regimen reminder task at {Time}.", DateTime.Now);
            try
            {
                var results = await service.SendNearDateAppointmentAsync(7);
                _logger.LogInformation($"There are {results.Count} appointment near the end date.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing regimen reminder task.");
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("Background Services are disposing.");
            _dailyTimer?.Dispose();
            base.Dispose();
        }
    }
}
