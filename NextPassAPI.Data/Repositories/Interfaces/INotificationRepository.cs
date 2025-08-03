using NextPassAPI.Data.Models;
using NextPassAPI.Data.Enums;

namespace NextPassAPI.Data.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        // Notification CRUD
        Task<Notification> CreateNotificationAsync(Notification notification);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<Notification>> GetUnreadNotificationsAsync(string userId);
        Task<bool> MarkAsReadAsync(string notificationId, string userId);
        Task<bool> MarkAllAsReadAsync(string userId);
        Task<bool> DeleteNotificationAsync(string notificationId, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        
        // Password Access Logs
        Task<PasswordAccessLog> LogPasswordAccessAsync(PasswordAccessLog accessLog);
        Task<List<PasswordAccessLog>> GetPasswordAccessHistoryAsync(string credentialId, int page = 1, int pageSize = 20);
        Task<List<PasswordAccessLog>> GetUserAccessHistoryAsync(string userId, int page = 1, int pageSize = 20);
        Task<List<PasswordAccessLog>> GetSuspiciousActivitiesAsync(string userId);
        
        // Security Alerts
        Task<SecurityAlert> CreateSecurityAlertAsync(SecurityAlert alert);
        Task<List<SecurityAlert>> GetSecurityAlertsAsync(string userId, AlertSeverity? severity = null);
        Task<bool> ResolveSecurityAlertAsync(string alertId, string resolvedBy);
        
        // Analytics
        Task<Dictionary<string, int>> GetNotificationStatsAsync(string userId, DateTime fromDate, DateTime toDate);
        Task<List<PasswordAccessLog>> GetRecentAccessesAsync(string userId, int count = 10);
        
        // Cleanup
        Task<bool> CleanupExpiredNotificationsAsync();
    }
}
