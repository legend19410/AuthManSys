using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using System.Security.Cryptography;

namespace AuthManSys.Application.Common.Services;

public class TwoFactorService : ITwoFactorService
{
    private const int CodeLength = 6;
    private const int ExpirationMinutes = 5;

    /// <summary>
    /// Generates a cryptographically secure 6-digit two-factor authentication code
    /// </summary>
    /// <returns>A 6-digit numeric code as string</returns>
    public string GenerateTwoFactorCode()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);

        // Convert to positive integer and get 6 digits
        var number = Math.Abs(BitConverter.ToInt32(bytes, 0));
        var code = (number % 1000000).ToString("D6");

        return code;
    }

    /// <summary>
    /// Validates if the provided code matches the stored code and is not expired
    /// </summary>
    /// <param name="storedCode">The code stored in the database</param>
    /// <param name="providedCode">The code provided by the user</param>
    /// <param name="expirationTime">When the code expires</param>
    /// <returns>True if the code is valid and not expired</returns>
    public bool ValidateTwoFactorCode(string? storedCode, string providedCode, DateTime? expirationTime)
    {
        // Check if codes exist
        if (string.IsNullOrEmpty(storedCode) || string.IsNullOrEmpty(providedCode))
        {
            return false;
        }

        // Check if code has expired
        if (!expirationTime.HasValue || JamaicaTimeHelper.Now > expirationTime.Value)
        {
            return false;
        }

        // Use constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(storedCode),
            System.Text.Encoding.UTF8.GetBytes(providedCode)
        );
    }

    /// <summary>
    /// Gets the expiration time for a two-factor code (5 minutes from now in Jamaica time)
    /// </summary>
    /// <returns>DateTime when the code should expire</returns>
    public DateTime GetCodeExpiration()
    {
        return JamaicaTimeHelper.Now.AddMinutes(ExpirationMinutes);
    }
}