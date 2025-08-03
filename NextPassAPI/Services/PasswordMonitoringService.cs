using NextPassAPI.Services.Interfaces;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Data.Enums;

namespace NextPassAPI.Services
{
    public class PasswordMonitoringService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PasswordMonitoringService> _logger;

        public PasswordMonitoringService(IServiceProvider serviceProvider, ILogger<PasswordMonitoringService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        var credentialRepository = scope.ServiceProvider.GetRequiredService<ICredentialRepository>();
                        var credentialLeakChecker = scope.ServiceProvider.GetRequiredService<ICredentialLeakChecker>();

                        // Check password change reminders
                        await CheckPasswordReminders(notificationService, credentialRepository);

                        // Check for compromised passwords
                        await CheckCompromisedPasswords(notificationService, credentialRepository, credentialLeakChecker);

                        // Cleanup old notifications
                        await notificationService.CleanupOldNotificationsAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during password monitoring");
                }

                // Run every 6 hours
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
        }

        private async Task CheckPasswordReminders(INotificationService notificationService, ICredentialRepository credentialRepository)
        {
            try
            {
                // This would need to be implemented based on your credential repository structure
                // For now, this is a placeholder showing the concept
                _logger.LogInformation("Checking password change reminders");

                // You would query all credentials and check if they need password change reminders
                // based on their PasswordChangeReminder field and LastPasswordChange date
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking password reminders");
            }
        }

        private async Task CheckCompromisedPasswords(INotificationService notificationService, ICredentialRepository credentialRepository, ICredentialLeakChecker credentialLeakChecker)
        {
            try
            {
                _logger.LogInformation("Checking for compromised passwords");

                // This would need to be implemented based on your credential repository structure
                // You would decrypt passwords (securely) and check them against breach databases
                // This is computationally expensive, so you might want to:
                // 1. Only check a subset of passwords each run
                // 2. Keep track of when passwords were last checked
                // 3. Prioritize high-value or frequently accessed credentials
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking compromised passwords");
            }
        }
    }
}
