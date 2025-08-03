using NextPassAPI.Data.Models;
using NextPassAPI.Data.Enums;

namespace NextPassAPI.Services.Interfaces
{
    public interface INotificationService
    {
        // Notification Management
        Task<Notification> CreateNotificationAsync(string userId, NotificationType type, string title, string message, NotificationPriority priority = NotificationPriority.Low, string? credentialId = null, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);

        // Password Access Logging
        Task LogPasswordAccessAsync(string userId, string credentialId, string credentialTitle, AccessType accessType, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null);
        Task<List<PasswordAccessLog>> GetPasswordAccessHistoryAsync(string credentialId, int page = 1, int pageSize = 20);
        Task<List<PasswordAccessLog>> GetUserAccessHistoryAsync(string userId, int page = 1, int pageSize = 20);

        // Security Alerts
        Task CreateSecurityAlertAsync(string userId, AlertType alertType, AlertSeverity severity, string title, string description, string? credentialId = null, DeviceInfo? deviceInfo = null);
        Task<List<SecurityAlert>> GetSecurityAlertsAsync(string userId, AlertSeverity? severity = null);

        // Specific Event Notifications
        Task NotifyCredentialCreatedAsync(string userId, string credentialTitle, DeviceInfo? deviceInfo = null);
        Task NotifyCredentialAccessedAsync(string userId, string credentialId, string credentialTitle, AccessType accessType, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null);
        Task NotifyCredentialUpdatedAsync(string userId, string credentialTitle, DeviceInfo? deviceInfo = null);
        Task NotifyCredentialDeletedAsync(string userId, string credentialTitle, DeviceInfo? deviceInfo = null);
        Task NotifyUserLoginAsync(string userId, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null);
        Task NotifyPasswordChangeReminderAsync(string userId, string credentialTitle, int daysOverdue);
        Task NotifyPasswordCompromisedAsync(string userId, string credentialId, string credentialTitle, string breachSource = "");
        Task NotifySuspiciousActivityAsync(string userId, string activity, DeviceInfo? deviceInfo = null, LocationInfo? locationInfo = null);

        // Analytics
        Task<Dictionary<string, int>> GetNotificationStatsAsync(string userId, DateTime fromDate, DateTime toDate);
        Task<List<PasswordAccessLog>> GetRecentAccessesAsync(string userId, int count = 10);

        // Background Tasks
        Task CheckPasswordRemindersAsync();
        Task CheckCompromisedPasswordsAsync();
        Task CleanupOldNotificationsAsync();
    }
}
