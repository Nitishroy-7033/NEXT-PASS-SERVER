namespace NextPassAPI.Data.Enums
{
    public enum NotificationType
    {
        CredentialCreated,
        CredentialAccessed,
        CredentialUpdated,
        CredentialDeleted,
        CredentialShared,
        CredentialUnshared,
        UserLogin,
        UserLogout,
        PasswordChangeReminder,
        PasswordCompromised,
        DataBreachAlert,
        SuspiciousActivity,
        NewDeviceLogin,
        UnknownLocationLogin,
        PasswordChanged,
        AccountSettingsChanged,
        TwoFactorEnabled,
        TwoFactorDisabled,
        BackupCreated,
        SecurityScanCompleted
    }

    public enum NotificationPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public enum AccessType
    {
        View,
        Copy,
        Edit,
        Delete,
        Share,
        Decrypt,
        Export
    }

    public enum AlertType
    {
        PasswordCompromised,
        DataBreach,
        SuspiciousLogin,
        UnauthorizedAccess,
        WeakPassword,
        ReusedPassword,
        ExpiredPassword,
        MultipleFailedLogins,
        NewDeviceAccess,
        UnknownLocationAccess,
        AccountLockout,
        SecuritySettingChanged
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical,
        Emergency
    }
}
