using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using NextPassAPI.Data.Enums;

namespace NextPassAPI.Data.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationPriority Priority { get; set; } = NotificationPriority.Low;
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        
        // Related entities
        public string? CredentialId { get; set; }
        public string? RelatedEntityId { get; set; }
        
        // Device and location information
        public DeviceInfo? DeviceInfo { get; set; }
        public LocationInfo? LocationInfo { get; set; }
        
        // Additional metadata
        public Dictionary<string, object>? Metadata { get; set; } = new Dictionary<string, object>();
        
        // Expiry for auto-cleanup
        public DateTime? ExpiresAt { get; set; }
    }

    public class DeviceInfo
    {
        public string? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string? DeviceType { get; set; } // Mobile, Desktop, Web, etc.
        public string? OperatingSystem { get; set; }
        public string? Browser { get; set; }
        public string? AppVersion { get; set; }
        public string? UserAgent { get; set; }
    }

    public class LocationInfo
    {
        public string? IpAddress { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Timezone { get; set; }
        public string? ISP { get; set; }
    }

    public class PasswordAccessLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        public string CredentialId { get; set; } = string.Empty;
        public string CredentialTitle { get; set; } = string.Empty;
        public AccessType AccessType { get; set; }
        public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
        
        public DeviceInfo? DeviceInfo { get; set; }
        public LocationInfo? LocationInfo { get; set; }
        
        // Security flags
        public bool IsSuspiciousActivity { get; set; } = false;
        public string? SuspiciousReason { get; set; }
        public bool IsFromTrustedDevice { get; set; } = false;
        public bool IsFromKnownLocation { get; set; } = false;
    }

    public class SecurityAlert
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        
        public string UserId { get; set; } = string.Empty;
        public AlertType AlertType { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
        
        // Related data
        public string? CredentialId { get; set; }
        public DeviceInfo? DeviceInfo { get; set; }
        public LocationInfo? LocationInfo { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; } = new Dictionary<string, object>();
    }
}
