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
        private Timer _timer;
        private readonly TimeSpan _dailyExecutionTime = new TimeSpan(0, 0, 0); // Midnight (00:00){
        public RegimenReminderBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RegimenReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Regimen Reminder Background Service is starting.");
            ScheduleDailyTask(stoppingToken);
            return Task.CompletedTask;
        }

        private void ScheduleDailyTask(CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            var nextRun = DateTime.Today.AddDays(1).Add(_dailyExecutionTime);

            if (now > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            var timeToNextRun = nextRun - now;
            _logger.LogInformation($"Next regimen reminder scheduled for {nextRun:yyyy-MM-dd HH:mm:ss}.");
            _timer = new Timer(
                async state => await ExecuteReminderTaskAsync(stoppingToken),
                null,
                timeToNextRun,
                Timeout.InfiniteTimeSpan);
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
            finally
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    ScheduleDailyTask(stoppingToken);
                }
            }
        }

        public override void Dispose()
        {
            _logger.LogInformation("Regimen Reminder Background Service is disposing.");
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
