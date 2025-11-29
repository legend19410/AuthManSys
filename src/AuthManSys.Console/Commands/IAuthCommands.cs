namespace AuthManSys.Console.Commands;

public interface IAuthCommands
{
    Task TestLoginAsync();
    Task TestRegistrationAsync();
    Task TestTokenValidationAsync();
    Task TestPasswordResetAsync();
    Task TestEmailConfirmationAsync();
}