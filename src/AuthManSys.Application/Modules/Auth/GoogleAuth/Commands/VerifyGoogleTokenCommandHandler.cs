using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;

namespace AuthManSys.Application.Modules.Auth.GoogleAuth.Commands;

public class VerifyGoogleTokenCommandHandler : IRequestHandler<VerifyGoogleTokenCommand, VerifyGoogleTokenResponse>
{
    private readonly IGoogleTokenService _googleTokenService;
    private readonly IIdentityProvider _identityProvider;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<VerifyGoogleTokenCommandHandler> _logger;

    public VerifyGoogleTokenCommandHandler(
        IGoogleTokenService googleTokenService,
        IIdentityProvider identityProvider,
        IUserRepository userRepository,
        ILogger<VerifyGoogleTokenCommandHandler> logger)
    {
        _googleTokenService = googleTokenService;
        _identityProvider = identityProvider;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<VerifyGoogleTokenResponse> Handle(VerifyGoogleTokenCommand request, CancellationToken cancellationToken)
    {
        // Verify the Google token
        var googlePayload = await _googleTokenService.VerifyTokenAsync(request.GoogleToken);
        if (googlePayload == null)
        {
            _logger.LogWarning("Invalid Google token provided");
            return new VerifyGoogleTokenResponse
            {
                IsValid = false,
                UserExists = false,
                IsSuccess = false,
                Message = "Invalid Google token"
            };
        }

        _logger.LogInformation("Google token verified successfully for email: {Email}", googlePayload.Email);

        // Check if user exists by Google ID first
        var userByGoogleId = await _userRepository.GetByGoogleIdAsync(googlePayload.Subject);
        if (userByGoogleId != null)
        {
            _logger.LogInformation("User found by Google ID: {GoogleId}", googlePayload.Subject);
            return new VerifyGoogleTokenResponse
            {
                IsValid = true,
                UserExists = true,
                UserId = userByGoogleId.Id,
                Email = userByGoogleId.Email,
                FirstName = userByGoogleId.FirstName,
                LastName = userByGoogleId.LastName,
                GoogleId = userByGoogleId.GoogleId,
                IsSuccess = true,
                Message = "User found by Google ID"
            };
        }

        // Check if user exists by email
        var userByEmail = await _identityProvider.FindByEmailAsync(googlePayload.Email);
        if (userByEmail != null)
        {
            _logger.LogInformation("User found by email: {Email}", googlePayload.Email);
            return new VerifyGoogleTokenResponse
            {
                IsValid = true,
                UserExists = true,
                UserId = userByEmail.Id,
                Email = userByEmail.Email,
                FirstName = userByEmail.FirstName,
                LastName = userByEmail.LastName,
                GoogleId = userByEmail.GoogleId,
                IsSuccess = true,
                Message = "User found by email (not linked to Google account)"
            };
        }

        // User does not exist in database
        _logger.LogInformation("No user found for Google email: {Email}", googlePayload.Email);
        return new VerifyGoogleTokenResponse
        {
            IsValid = true,
            UserExists = false,
            Email = googlePayload.Email,
            FirstName = googlePayload.GivenName,
            LastName = googlePayload.FamilyName,
            GoogleId = googlePayload.Subject,
            IsSuccess = true,
            Message = "Google token is valid but user does not exist in database"
        };
    }
}