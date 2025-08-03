using NextPassAPI.Data.Models;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Services.Interfaces;
using NextPassAPI.Data.Enums;
using Microsoft.AspNetCore.Http;

namespace NextPassAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ICredentialRepository _credentialRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICredentialLeakChecker _credentialLeakChecker;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            INotificationRepository notificationRepository,
            ICredentialRepository credentialRepository,
            IUserRepository userRepository,
            ICredentialLeakChecker credentialLeakChecker,
            IHttpContextAccessor httpContextAccessor)
        {
            _notificationRepository = notificationRepository;
            _credentialRepository = credentialRepository;
            _userRepository = userRepository;
            _credentialLeakChecker = credentialLeakChecker;
            _httpContextAccessor = httpContextAccessor;
        }

        // Notification Management
        public async Task<Notification> CreateNotificationAsync(string userId, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Low, string? credentialId = null, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                Priority = priority,
                CredentialId = credentialId,
                DeviceInfo = deviceInfo,
                LocationInfo = locationInfo,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30) // Auto-expire after 30 days
            };

            return await _notificationRepository.CreateNotificationAsync(notification);
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _notificationRepository.GetUserNotificationsAsync(userId, page, pageSize);
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _notificationRepository.GetUnreadNotificationsAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            return await _notificationRepository.MarkAsReadAsync(notificationId, userId);
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        // Password Access Logging
        public async Task LogPasswordAccessAsync(string userId, string credentialId, string credentialTitle, AccessType accessType, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null)
        {
            var accessLog = new PasswordAccessLog
            {
                UserId = userId,
                CredentialId = credentialId,
                CredentialTitle = credentialTitle,
                AccessType = accessType,
                AccessedAt = DateTime.UtcNow,
                DeviceInfo = deviceInfo,
                LocationInfo = locationInfo,
                IsFromTrustedDevice = await IsFromTrustedDeviceAsync(userId, deviceInfo),
                IsFromKnownLocation = await IsFromKnownLocationAsync(userId, locationInfo)
            };

            // Check for suspicious activity
            await DetectSuspiciousActivityAsync(accessLog);

            await _notificationRepository.LogPasswordAccessAsync(accessLog);

            // Create notification for password access
            await NotifyCredentialAccessedAsync(userId, credentialId, credentialTitle, accessType, deviceInfo, locationInfo);
        }

        public async Task<List<PasswordAccessLog>> GetPasswordAccessHistoryAsync(string credentialId, int page = 1, int pageSize = 20)
        {
            return await _notificationRepository.GetPasswordAccessHistoryAsync(credentialId, page, pageSize);
        }

        public async Task<List<PasswordAccessLog>> GetUserAccessHistoryAsync(string userId, int page = 1, int pageSize = 20)
        {
            return await _notificationRepository.GetUserAccessHistoryAsync(userId, page, pageSize);
        }

        // Security Alerts
        public async Task CreateSecurityAlertAsync(string userId, AlertType alertType, AlertSeverity severity, string title, string description, string? credentialId = null, DeviceInfo? deviceInfo = null)
        {
            var alert = new SecurityAlert
            {
                UserId = userId,
                AlertType = alertType,
                Severity = severity,
                Title = title,
                Description = description,
                CredentialId = credentialId,
                DeviceInfo = deviceInfo,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.CreateSecurityAlertAsync(alert);

            // Also create a notification
            var priority = severity switch
            {
                AlertSeverity.Critical or AlertSeverity.Emergency => NotificationPriority.Critical,
                AlertSeverity.Warning => NotificationPriority.High,
                _ => NotificationPriority.Medium
            };

            await CreateNotificationAsync(userId, NotificationType.SuspiciousActivity, title, description, priority, credentialId, deviceInfo);
        }

        public async Task<List<SecurityAlert>> GetSecurityAlertsAsync(string userId, AlertSeverity? severity = null)
        {
            return await _notificationRepository.GetSecurityAlertsAsync(userId, severity);
        }

        // Specific Event Notifications
        public async Task NotifyCredentialCreatedAsync(string userId, string credentialTitle, DeviceInfo? deviceInfo = null)
        {
            await CreateNotificationAsync(
                userId,
                NotificationType.CredentialCreated,
                "New Credential Created",
                $"You created a new credential: {credentialTitle}",
                NotificationPriority.Low,
                deviceInfo: deviceInfo
            );
        }

        public async Task NotifyCredentialAccessedAsync(string userId, string credentialId, string credentialTitle, AccessType accessType, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null)
        {
            var actionText = accessType switch
            {
                AccessType.View => "viewed",
                AccessType.Copy => "copied",
                AccessType.Edit => "edited",
                AccessType.Delete => "deleted",
                AccessType.Share => "shared",
                AccessType.Decrypt => "decrypted",
                AccessType.Export => "exported",
                _ => "accessed"
            };

            var deviceText = deviceInfo?.DeviceName ?? "Unknown device";
            var locationText = locationInfo?.City ?? "Unknown location";

            await CreateNotificationAsync(
                userId,
                NotificationType.CredentialAccessed,
                "Credential Accessed",
                $"Your credential '{credentialTitle}' was {actionText} from {deviceText} in {locationText}",
                NotificationPriority.Medium,
                credentialId,
                deviceInfo,
                locationInfo
            );
        }

        public async Task NotifyCredentialUpdatedAsync(string userId, string credentialTitle, DeviceInfo? deviceInfo = null)
        {
            await CreateNotificationAsync(
                userId,
                NotificationType.CredentialUpdated,
                "Credential Updated",
                $"Your credential '{credentialTitle}' was updated",
                NotificationPriority.Low,
                deviceInfo: deviceInfo
            );
        }

        public async Task NotifyCredentialDeletedAsync(string userId, string credentialTitle, DeviceInfo? deviceInfo = null)
        {
            await CreateNotificationAsync(
                userId,
                NotificationType.CredentialDeleted,
                "Credential Deleted",
                $"Your credential '{credentialTitle}' was deleted",
                NotificationPriority.Medium,
                deviceInfo: deviceInfo
            );
        }

        public async Task NotifyUserLoginAsync(string userId, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null)
        {
            var deviceText = deviceInfo?.DeviceName ?? "Unknown device";
            var locationText = locationInfo?.City ?? "Unknown location";

            await CreateNotificationAsync(
                userId,
                NotificationType.UserLogin,
                "New Login",
                $"You logged in from {deviceText} in {locationText}",
                NotificationPriority.Low,
                deviceInfo: deviceInfo,
                locationInfo: locationInfo
            );
        }

        public async Task NotifyPasswordChangeReminderAsync(string userId, string credentialTitle, int daysOverdue)
        {
            var priority = daysOverdue > 30 ? NotificationPriority.High : NotificationPriority.Medium;
            var message = daysOverdue > 0 
                ? $"Password for '{credentialTitle}' is {daysOverdue} days overdue for change"
                : $"Time to change password for '{credentialTitle}'";

            await CreateNotificationAsync(
                userId,
                NotificationType.PasswordChangeReminder,
                "Password Change Reminder",
                message,
                priority
            );
        }

        public async Task NotifyPasswordCompromisedAsync(string userId, string credentialId, string credentialTitle, string breachSource = "")
        {
            var message = string.IsNullOrEmpty(breachSource)
                ? $"Password for '{credentialTitle}' has been found in a data breach"
                : $"Password for '{credentialTitle}' was found in the {breachSource} data breach";

            await CreateNotificationAsync(
                userId,
                NotificationType.PasswordCompromised,
                "Password Compromised",
                message,
                NotificationPriority.Critical,
                credentialId
            );

            // Also create security alert
            await CreateSecurityAlertAsync(
                userId,
                AlertType.PasswordCompromised,
                AlertSeverity.Critical,
                "Password Found in Data Breach",
                message,
                credentialId
            );
        }

        public async Task NotifySuspiciousActivityAsync(string userId, string activity, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null)
        {
            await CreateNotificationAsync(
                userId,
                NotificationType.SuspiciousActivity,
                "Suspicious Activity Detected",
                activity,
                NotificationPriority.High,
                deviceInfo: deviceInfo,
                locationInfo: locationInfo
            );
        }

        // Analytics
        public async Task<Dictionary<string, int>> GetNotificationStatsAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            return await _notificationRepository.GetNotificationStatsAsync(userId, fromDate, toDate);
        }

        public async Task<List<PasswordAccessLog>> GetRecentAccessesAsync(string userId, int count = 10)
        {
            return await _notificationRepository.GetRecentAccessesAsync(userId, count);
        }

        // Background Tasks
        public async Task CheckPasswordRemindersAsync()
        {
            // Implementation for checking password change reminders
            // This would typically be called by a background service
        }

        public async Task CheckCompromisedPasswordsAsync()
        {
            // Implementation for checking compromised passwords
            // This would typically be called by a background service
        }

        public async Task CleanupOldNotificationsAsync()
        {
            await _notificationRepository.CleanupExpiredNotificationsAsync();
        }

        // Helper Methods
        private async Task<bool> IsFromTrustedDeviceAsync(string userId, DeviceInfo? deviceInfo)
        {
            if (deviceInfo?.DeviceId == null) return false;
            
            // Check if device is in user's trusted devices list
            // Implementation depends on your user model
            return false; // Placeholder
        }

        private async Task<bool> IsFromKnownLocationAsync(string userId, LocationInfo? locationInfo)
        {
            if (locationInfo?.City == null) return false;
            
            // Check if location is in user's known locations
            // Implementation depends on your requirements
            return false; // Placeholder
        }

        private async Task DetectSuspiciousActivityAsync(PasswordAccessLog accessLog)
        {
            // Implement suspicious activity detection logic
            // Examples:
            // - Multiple rapid accesses
            // - Access from new/untrusted devices
            // - Access from unusual locations
            // - Access at unusual times

            var recentAccesses = await _notificationRepository.GetRecentAccessesAsync(accessLog.UserId, 10);
            
            // Check for rapid successive accesses (more than 5 in 1 minute)
            var recentCount = recentAccesses.Count(a => a.AccessedAt > DateTime.UtcNow.AddMinutes(-1));
            if (recentCount > 5)
            {
                accessLog.IsSuspiciousActivity = true;
                accessLog.SuspiciousReason = "Multiple rapid accesses detected";
            }

            // Check for access from new device
            if (!accessLog.IsFromTrustedDevice && accessLog.DeviceInfo?.DeviceId != null)
            {
                accessLog.IsSuspiciousActivity = true;
                accessLog.SuspiciousReason = "Access from new/untrusted device";
            }

            // Add more detection logic as needed
        }

        private DeviceInfo? ExtractDeviceInfo()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            var deviceId = context.Request.Headers["X-Device-ID"].ToString();

            return new DeviceInfo
            {
                DeviceId = deviceId,
                UserAgent = userAgent,
                // Parse user agent to extract device info
                // This is a simplified version - you might want to use a library like UAParser
            };
        }

        private LocationInfo? ExtractLocationInfo()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            
            return new LocationInfo
            {
                IpAddress = ipAddress
                // You would integrate with a geolocation service here
                // to get city, country, etc. from IP address
            };
        }
    }
}
