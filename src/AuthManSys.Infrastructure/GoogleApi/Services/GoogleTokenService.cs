using AuthManSys.Application.Common.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthManSys.Infrastructure.GoogleApi.Services;

public class GoogleTokenService : IGoogleTokenService
{
    private readonly string _googleClientId;
    private readonly ILogger<GoogleTokenService> _logger;

    public GoogleTokenService(IConfiguration configuration, ILogger<GoogleTokenService> logger)
    {
        _googleClientId = configuration["GoogleAuth:ClientId"] ?? throw new ArgumentNullException(nameof(configuration), "GoogleAuth:ClientId is required");
        _logger = logger;
    }

    public async Task<GoogleJsonWebSignature.Payload?> VerifyTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _googleClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            _logger.LogInformation("Successfully verified Google token for user {Email}", payload.Email);
            return payload;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning("Invalid Google token: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Google token");
            return null;
        }
    }
}