using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextPassAPI.Data.Models
{
    public class Credential
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string SiteUrl { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserId { get; set; }
        public bool IsFavorite { get; set; } = false;
        public int PasswordChangeReminder { get; set; } = 30;
        public string? PasswordStrength { get; set; }
        public bool IsPasswordCompromised { get; set; } = false;
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastPasswordChange { get; set; }
        public List<PasswordAccess> PasswordAccessed { get; set; } = new List<PasswordAccess>();
        public List<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();
        public bool TwoFactorAuthEnabled { get; set; } = false;
        public List<SecurityQuestion> SecurityQuestions { get; set; } = new List<SecurityQuestion>();
        public NotificationSettings Notifications { get; set; } = new NotificationSettings();
        public List<TrustedDevice> TrustedDevices { get; set; } = new List<TrustedDevice>();
        public string? BackupEmail { get; set; }
        public string Notes { get; set; }
        public List<string> SharedWith { get; set; } = new List<string>();
    }
}




public class PasswordHistory
{
    public string? OldPassword { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ChangeDate { get; set; } = DateTime.UtcNow;
    public string? ChangeReason { get; set; }
}

public class SecurityQuestion
{
    public string? Question { get; set; }
    public string? Answer { get; set; }
}

public class NotificationSettings
{
    public bool PasswordAccessed { get; set; } = false;
    public bool PasswordChanged { get; set; } = false;
    public bool PasswordLeaked { get; set; } = false;
}

public class TrustedDevice
{
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
}