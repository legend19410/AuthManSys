using System;

namespace AuthManSys.Application.Common.Interfaces;

public interface ISecurityOptions
{
    public int PasswordRequiredLength { get; set; }
    public int PasswordRequiredUniqueChars { get; set; }
    public bool PasswordRequireDigit { get; set; }
    public bool PasswordRequireUppercase { get; set; }
    public int MaxFailedAttempts { get; set; }
    public int PasswordExpireDays { get; set; }
}
