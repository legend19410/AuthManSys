using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AuthManSys.Infrastructure.GoogleApi.Configuration;

namespace AuthManSys.Infrastructure.GoogleApi.Authentication;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleApiSettings _settings;
    private readonly ILogger<GoogleAuthService> _logger;
    private GoogleCredential? _cachedCredential;

    public GoogleAuthService(IOptions<GoogleApiSettings> settings, ILogger<GoogleAuthService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<GoogleCredential> GetCredentialAsync()
    {
        if (_cachedCredential != null)
        {
            return _cachedCredential;
        }

        try
        {
            if (string.IsNullOrEmpty(_settings.ServiceAccountKeyPath))
            {
                throw new InvalidOperationException("ServiceAccountKeyPath is not configured in GoogleApi settings");
            }

            if (!File.Exists(_settings.ServiceAccountKeyPath))
            {
                throw new FileNotFoundException($"Service account key file not found at: {_settings.ServiceAccountKeyPath}");
            }

            _logger.LogDebug("Loading Google service account credentials from: {Path}", _settings.ServiceAccountKeyPath);

            var credential = GoogleCredential.FromFile(_settings.ServiceAccountKeyPath);

            if (_settings.Scopes?.Length > 0)
            {
                credential = credential.CreateScoped(_settings.Scopes);
            }
            else
            {
                credential = credential.CreateScoped(GoogleApiScopes.DefaultScopes);
            }

            _cachedCredential = credential;
            _logger.LogInformation("Successfully loaded Google API credentials");

            return credential;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load Google API credentials");
            throw;
        }
    }

    public BaseClientService.Initializer GetServiceInitializer(string applicationName)
    {
        var credential = GetCredentialAsync().GetAwaiter().GetResult();

        return new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        };
    }

    public async Task<bool> ValidateCredentialsAsync()
    {
        try
        {
            var credential = await GetCredentialAsync();
            var initializer = GetServiceInitializer(_settings.ApplicationName);

            // Test credentials by creating a Drive service and making a simple request
            var driveService = new DriveService(initializer);
            var aboutRequest = driveService.About.Get();
            aboutRequest.Fields = "user";

            var about = await aboutRequest.ExecuteAsync();

            _logger.LogInformation("Google API credentials validated successfully for user: {Email}",
                about.User?.EmailAddress ?? "Unknown");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate Google API credentials");
            return false;
        }
    }
}