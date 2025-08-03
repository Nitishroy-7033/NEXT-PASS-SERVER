using MongoDB.Driver;
using MongoDB.Bson;
using NextPassAPI.Data.DbContexts;
using NextPassAPI.Data.Models;
using NextPassAPI.Data.Repositories.Interfaces;
using NextPassAPI.Data.Enums;

namespace NextPassAPI.Data.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly IMongoCollection<Notification> _notifications;
        private readonly IMongoCollection<PasswordAccessLog> _accessLogs;
        private readonly IMongoCollection<SecurityAlert> _securityAlerts;

        public NotificationRepository(
            MongoDbContext<Notification> notificationContext,
            MongoDbContext<PasswordAccessLog> accessLogContext,
            MongoDbContext<SecurityAlert> securityAlertContext)
        {
            _notifications = notificationContext.GetCollection();
            _accessLogs = accessLogContext.GetCollection();
            _securityAlerts = securityAlertContext.GetCollection();
        }

        // Notification CRUD
        public async Task<Notification> CreateNotificationAsync(Notification notification)
        {
            await _notifications.InsertOneAsync(notification);
            return notification;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;
            return await _notifications
                .Find(n => n.UserId == userId)
                .SortByDescending(n => n.CreatedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(string userId)
        {
            return await _notifications
                .Find(n => n.UserId == userId && !n.IsRead)
                .SortByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(string notificationId, string userId)
        {
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            var result = await _notifications.UpdateOneAsync(
                n => n.Id == notificationId && n.UserId == userId,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> MarkAllAsReadAsync(string userId)
        {
            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow);

            var result = await _notifications.UpdateManyAsync(
                n => n.UserId == userId && !n.IsRead,
                update);

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteNotificationAsync(string notificationId, string userId)
        {
            var result = await _notifications.DeleteOneAsync(
                n => n.Id == notificationId && n.UserId == userId);

            return result.DeletedCount > 0;
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return (int)await _notifications.CountDocumentsAsync(
                n => n.UserId == userId && !n.IsRead);
        }

        // Password Access Logs
        public async Task<PasswordAccessLog> LogPasswordAccessAsync(PasswordAccessLog accessLog)
        {
            await _accessLogs.InsertOneAsync(accessLog);
            return accessLog;
        }

        public async Task<List<PasswordAccessLog>> GetPasswordAccessHistoryAsync(string credentialId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;
            return await _accessLogs
                .Find(log => log.CredentialId == credentialId)
                .SortByDescending(log => log.AccessedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<PasswordAccessLog>> GetUserAccessHistoryAsync(string userId, int page = 1, int pageSize = 20)
        {
            var skip = (page - 1) * pageSize;
            return await _accessLogs
                .Find(log => log.UserId == userId)
                .SortByDescending(log => log.AccessedAt)
                .Skip(skip)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task<List<PasswordAccessLog>> GetSuspiciousActivitiesAsync(string userId)
        {
            return await _accessLogs
                .Find(log => log.UserId == userId && log.IsSuspiciousActivity)
                .SortByDescending(log => log.AccessedAt)
                .Limit(50)
                .ToListAsync();
        }

        // Security Alerts
        public async Task<SecurityAlert> CreateSecurityAlertAsync(SecurityAlert alert)
        {
            await _securityAlerts.InsertOneAsync(alert);
            return alert;
        }

        public async Task<List<SecurityAlert>> GetSecurityAlertsAsync(string userId, AlertSeverity? severity = null)
        {
            var filterBuilder = Builders<SecurityAlert>.Filter;
            var filter = filterBuilder.Eq(a => a.UserId, userId);

            if (severity.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(a => a.Severity, severity.Value));
            }

            return await _securityAlerts
                .Find(filter)
                .SortByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ResolveSecurityAlertAsync(string alertId, string resolvedBy)
        {
            var update = Builders<SecurityAlert>.Update
                .Set(a => a.IsResolved, true)
                .Set(a => a.ResolvedAt, DateTime.UtcNow)
                .Set(a => a.ResolvedBy, resolvedBy);

            var result = await _securityAlerts.UpdateOneAsync(
                a => a.Id == alertId,
                update);

            return result.ModifiedCount > 0;
        }

        // Analytics
        public async Task<Dictionary<string, int>> GetNotificationStatsAsync(string userId, DateTime fromDate, DateTime toDate)
        {
            var pipeline = new[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    {"UserId", userId},
                    {"CreatedAt", new BsonDocument
                        {
                            {"$gte", fromDate},
                            {"$lte", toDate}
                        }
                    }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    {"_id", "$Type"},
                    {"count", new BsonDocument("$sum", 1)}
                })
            };

            var results = await _notifications.Aggregate<BsonDocument>(pipeline).ToListAsync();
            var stats = new Dictionary<string, int>();

            foreach (var result in results)
            {
                var type = result["_id"].AsString;
                var count = result["count"].AsInt32;
                stats[type] = count;
            }

            return stats;
        }

        public async Task<List<PasswordAccessLog>> GetRecentAccessesAsync(string userId, int count = 10)
        {
            return await _accessLogs
                .Find(log => log.UserId == userId)
                .SortByDescending(log => log.AccessedAt)
                .Limit(count)
                .ToListAsync();
        }

        // Cleanup
        public async Task<bool> CleanupExpiredNotificationsAsync()
        {
            var result = await _notifications.DeleteManyAsync(
                n => n.ExpiresAt != null && n.ExpiresAt < DateTime.UtcNow);

            return result.DeletedCount > 0;
        }
    }
}
