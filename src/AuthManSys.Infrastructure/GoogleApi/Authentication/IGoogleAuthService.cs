using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace AuthManSys.Infrastructure.GoogleApi.Authentication;

public interface IGoogleAuthService
{
    Task<GoogleCredential> GetCredentialAsync();
    BaseClientService.Initializer GetServiceInitializer(string applicationName);
    Task<bool> ValidateCredentialsAsync();
}