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
        private Timer? _dailyTimer;
        private readonly TimeSpan _executionInterval = TimeSpan.FromMinutes(1);
        private volatile bool _isRunning;

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
            _dailyTimer = new Timer(
                async _ => await ExecuteDailyTasksAsync(stoppingToken),
                null,
                TimeSpan.Zero, // Start immediately
                _executionInterval); // Repeat every minute
        }

        private async Task ExecuteDailyTasksAsync(CancellationToken stoppingToken)
        {
            // Prevent concurrent execution
            if (_isRunning || stoppingToken.IsCancellationRequested)
                return;

            try
            {
                _isRunning = true;

                using var scope = _scopeFactory.CreateScope();
                var patientArvRegimenService = scope.ServiceProvider.GetRequiredService<IPatientArvRegimenService>();
                var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();

                // Execute tasks sequentially
                await ArvRegimenReminderAsync(patientArvRegimenService, stoppingToken);
                if (!stoppingToken.IsCancellationRequested)
                    await CancelPastDateAppointmentsAsync(appointmentService, stoppingToken);
                if (!stoppingToken.IsCancellationRequested)
                    await AppontmentReminderAsync(appointmentService, stoppingToken);

                var nextRun = DateTime.Now.Add(_executionInterval);
                _logger.LogInformation($"Next tasks scheduled for {nextRun:yyyy-MM-dd HH:mm:ss}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing daily tasks.");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private async Task ArvRegimenReminderAsync(IPatientArvRegimenService service, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing ARV regimen reminder task at {Time}.", DateTime.Now);
            try
            {
                var results = await service.SendEndDateReminderNotificationsAsync(7);
                _logger.LogInformation($"Sent {results.Count} regimen end date reminders.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing ARV regimen reminder task.");
            }
        }

        private async Task CancelPastDateAppointmentsAsync(IAppointmentService service, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing cancel past date appointments task at {Time}.", DateTime.Now);
            try
            {
                await service.CancelPastDateAppointmentsAsync();
                _logger.LogInformation("Successfully cancelled past date appointments.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing cancel past date appointments task.");
            }
        }

        private async Task AppontmentReminderAsync(IAppointmentService service, CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing appointment reminder task at {Time}.", DateTime.Now);
            try
            {
                var results = await service.SendNearDateAppointmentAsync(7);
                _logger.LogInformation($"Found {results.Count} appointments near the end date.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing appointment reminder task.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Background Services are stopping.");
            
            _dailyTimer?.Change(Timeout.Infinite, 0);
            
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("Background Services are disposing.");
            _dailyTimer?.Dispose();
            base.Dispose();
        }
    }
}
