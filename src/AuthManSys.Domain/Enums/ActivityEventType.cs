namespace AuthManSys.Domain.Enums;

public enum ActivityEventType
{
    // Authentication Events
    LoginSuccess,
    LoginFailed,
    LoginError,
    LogoutSuccess,

    // Two-Factor Authentication Events
    TwoFactorRequired,
    TwoFactorSuccess,
    TwoFactorFailed,
    TwoFactorEnabled,
    TwoFactorDisabled,
    TwoFactorCodeSent,
    TwoFactorCodeRequestFailed,

    // User Registration Events
    UserRegistered,
    UserRegistrationFailed,
    UserCreated,

    // Email Verification Events
    EmailConfirmationSent,
    EmailConfirmed,
    EmailConfirmationFailed,

    // Password Management Events
    PasswordChanged,
    PasswordResetRequested,
    PasswordResetSuccess,
    PasswordResetFailed,

    // Account Management Events
    AccountLocked,
    AccountUnlocked,
    AccountDeleted,
    AccountRestored,
    AccountLinked,

    // Profile Management Events
    ProfileUpdated,
    ProfileUpdateFailed,

    // Role Management Events
    RoleAssigned,
    RoleRemoved,
    RoleRevoked,
    RoleCreated,

    // Permission Events
    PermissionGranted,
    PermissionRevoked,
    PermissionCreated,
    BulkPermissionGrant,
    BulkPermissionRevoke,

    // Password Events
    PasswordReset,

    // Security Events
    SuspiciousActivity,
    SecurityViolation,
    UnauthorizedAccess,

    // System Events
    SystemError,
    SystemMaintenance,

    // API Events
    ApiAccessGranted,
    ApiAccessDenied,

    // Data Events
    DataExport,
    DataImport,
    DataPurged
}