namespace AuthManSys.Application.Common.Interfaces;

public interface ITwoFactorService
{
    /// <summary>
    /// Generates a 6-digit two-factor authentication code
    /// </summary>
    /// <returns>A 6-digit numeric code as string</returns>
    string GenerateTwoFactorCode();

    /// <summary>
    /// Validates if the provided code matches the stored code and is not expired
    /// </summary>
    /// <param name="storedCode">The code stored in the database</param>
    /// <param name="providedCode">The code provided by the user</param>
    /// <param name="expirationTime">When the code expires</param>
    /// <returns>True if the code is valid and not expired</returns>
    bool ValidateTwoFactorCode(string? storedCode, string providedCode, DateTime? expirationTime);

    /// <summary>
    /// Gets the expiration time for a two-factor code (default 5 minutes from now)
    /// </summary>
    /// <returns>DateTime when the code should expire</returns>
    DateTime GetCodeExpiration();
}