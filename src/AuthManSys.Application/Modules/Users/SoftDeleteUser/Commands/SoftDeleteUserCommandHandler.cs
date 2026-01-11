using MediatR;
using Microsoft.Extensions.Logging;
using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Application.Common.Models.Responses;

namespace AuthManSys.Application.Modules.Users.SoftDeleteUser.Commands;

public class SoftDeleteUserCommandHandler : IRequestHandler<SoftDeleteUserCommand, SoftDeleteUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SoftDeleteUserCommandHandler> _logger;

    public SoftDeleteUserCommandHandler(
        IUserRepository userRepository,
        ILogger<SoftDeleteUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SoftDeleteUserResponse> Handle(SoftDeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find the user first
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning("User {Username} not found for soft delete", request.Username);
                return new SoftDeleteUserResponse
                {
                    IsDeleted = false,
                    Message = "User not found"
                };
            }

            var result = await _userRepository.SoftDeleteAsync(user, request.DeletedBy);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Username} soft deleted successfully by {DeletedBy}", request.Username, request.DeletedBy);
                return new SoftDeleteUserResponse
                {
                    IsDeleted = true,
                    Message = "User soft deleted successfully",
                    Username = request.Username,
                    DeletedAt = JamaicaTimeHelper.Now,
                    DeletedBy = request.DeletedBy
                };
            }

            _logger.LogWarning("Failed to soft delete user {Username}: {Errors}", request.Username, string.Join(", ", result.Errors));
            return new SoftDeleteUserResponse
            {
                IsDeleted = false,
                Message = $"Failed to delete user: {string.Join(", ", result.Errors)}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting user {Username}", request.Username);
            return new SoftDeleteUserResponse
            {
                IsDeleted = false,
                Message = "An error occurred while deleting the user"
            };
        }
    }
}