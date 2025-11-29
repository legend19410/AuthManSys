using AuthManSys.Domain.Enums;

namespace AuthManSys.Application.Common.Helpers;

public static class ActivityEventTagMapper
{
    public static string GetEventTag(ActivityEventType eventType)
    {
        return eventType switch
        {
            // Authentication Events
            ActivityEventType.LoginSuccess => "User signed in",
            ActivityEventType.LoginFailed => "Sign in failed",
            ActivityEventType.LoginError => "Sign in error",
            ActivityEventType.LogoutSuccess => "User signed out",

            // Two-Factor Authentication Events
            ActivityEventType.TwoFactorRequired => "Two-factor authentication required",
            ActivityEventType.TwoFactorSuccess => "Two-factor authentication verified",
            ActivityEventType.TwoFactorFailed => "Two-factor authentication failed",
            ActivityEventType.TwoFactorEnabled => "Two-factor authentication enabled",
            ActivityEventType.TwoFactorDisabled => "Two-factor authentication disabled",
            ActivityEventType.TwoFactorCodeSent => "Two-factor code sent",
            ActivityEventType.TwoFactorCodeRequestFailed => "Two-factor code request failed",

            // User Registration Events
            ActivityEventType.UserRegistered => "Account created",
            ActivityEventType.UserRegistrationFailed => "Account creation failed",

            // Email Verification Events
            ActivityEventType.EmailConfirmationSent => "Email verification sent",
            ActivityEventType.EmailConfirmed => "Email verified",
            ActivityEventType.EmailConfirmationFailed => "Email verification failed",

            // Password Management Events
            ActivityEventType.PasswordChanged => "Password changed",
            ActivityEventType.PasswordResetRequested => "Password reset requested",
            ActivityEventType.PasswordResetSuccess => "Password reset successfully",
            ActivityEventType.PasswordResetFailed => "Password reset failed",

            // Account Management Events
            ActivityEventType.AccountLocked => "Account locked",
            ActivityEventType.AccountUnlocked => "Account unlocked",
            ActivityEventType.AccountDeleted => "Account deleted",
            ActivityEventType.AccountRestored => "Account restored",

            // Profile Management Events
            ActivityEventType.ProfileUpdated => "Profile updated",
            ActivityEventType.ProfileUpdateFailed => "Profile update failed",

            // Role Management Events
            ActivityEventType.RoleAssigned => "Role assigned",
            ActivityEventType.RoleRemoved => "Role removed",
            ActivityEventType.RoleCreated => "Role created",

            // Permission Events
            ActivityEventType.PermissionGranted => "Permission granted",
            ActivityEventType.PermissionRevoked => "Permission revoked",

            // Security Events
            ActivityEventType.SuspiciousActivity => "Suspicious activity detected",
            ActivityEventType.SecurityViolation => "Security violation",
            ActivityEventType.UnauthorizedAccess => "Unauthorized access attempt",

            // System Events
            ActivityEventType.SystemError => "System error occurred",
            ActivityEventType.SystemMaintenance => "System maintenance",

            // API Events
            ActivityEventType.ApiAccessGranted => "API access granted",
            ActivityEventType.ApiAccessDenied => "API access denied",

            // Data Events
            ActivityEventType.DataExport => "Data exported",
            ActivityEventType.DataImport => "Data imported",
            ActivityEventType.DataPurged => "Data purged",

            _ => "Unknown activity"
        };
    }
}