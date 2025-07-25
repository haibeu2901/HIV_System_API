using HIV_System_API_Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HIV_System_API_Backend.Services
{
    public class MedicationAlarmBackgroundService : BackgroundService
    {
        private readonly ILogger<MedicationAlarmBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // Check every 5 minutes

        public MedicationAlarmBackgroundService(
            ILogger<MedicationAlarmBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Medication Alarm Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Processing medication alarms at: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var medicationAlarmService = scope.ServiceProvider.GetRequiredService<IMedicationAlarmService>();
                        await medicationAlarmService.ProcessMedicationAlarmsAsync();
                    }

                    _logger.LogInformation("Medication alarms processed successfully at: {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing medication alarms at: {time}", DateTimeOffset.Now);
                }

                // Wait for the next interval
                await Task.Delay(_interval, stoppingToken);
            }

            _logger.LogInformation("Medication Alarm Background Service is stopping.");
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Medication Alarm Background Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}