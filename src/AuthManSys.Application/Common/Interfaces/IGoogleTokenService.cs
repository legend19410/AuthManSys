using Google.Apis.Auth;

namespace AuthManSys.Application.Common.Interfaces;

public interface IGoogleTokenService
{
    Task<GoogleJsonWebSignature.Payload?> VerifyTokenAsync(string idToken);
}