using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Services;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;
using System.IdentityModel.Tokens.Jwt;

namespace AuthManSys.Application.Modules.Auth.RefreshToken.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtService jwtService,
        ITokenRepository tokenRepository,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _tokenRepository = tokenRepository;
        _logger = logger;
    }

    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get user by refresh token
            var user = await _tokenRepository.GetUserByRefreshTokenAsync(request.RefreshToken);
            if (user == null)
            {
                _logger.LogWarning("Invalid refresh token provided");
                return new RefreshTokenResponse
                {
                    IsSuccess = false,
                    Message = "Invalid refresh token"
                };
            }

            // Get user roles for token generation
            var userRoles = await _userRepository.GetUserRolesAsync(user);

            // Generate new JWT token
            var newToken = _jwtService.GenerateAccessToken(user.UserName!, user.Email!, user.Id, userRoles);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var newJwtToken = jwtTokenHandler.ReadJwtToken(newToken);

            // Validate the current refresh token
            var isValidRefreshToken = await _tokenRepository.ValidateRefreshTokenAsync(
                request.RefreshToken,
                newJwtToken.Id);

            if (!isValidRefreshToken)
            {
                _logger.LogWarning("Refresh token validation failed for user {UserId}", user.UserId);
                return new RefreshTokenResponse
                {
                    IsSuccess = false,
                    Message = "Invalid or expired refresh token"
                };
            }

            // Generate new refresh token
            var newRefreshToken = await _tokenRepository.GenerateRefreshTokenAsync(user, newJwtToken.Id);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", user.UserId);

            return new RefreshTokenResponse
            {
                IsSuccess = true,
                Token = newToken,
                RefreshToken = newRefreshToken,
                Message = "Token refreshed successfully",
                TokenExpiration = newJwtToken.ValidTo,
                RefreshTokenExpiration = JamaicaTimeHelper.Now.AddDays(30) // This should match your config
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while refreshing token");
            return new RefreshTokenResponse
            {
                IsSuccess = false,
                Message = "An error occurred while refreshing the token"
            };
        }
    }
}