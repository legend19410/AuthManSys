namespace AuthManSys.Console.Commands;

public interface IUserCommands
{
    Task ListUsersAsync();
    Task CreateUserAsync();
    Task DeleteUserAsync();
    Task UpdateUserAsync();
    Task FindUserAsync();
}