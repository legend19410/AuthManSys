namespace AuthManSys.Console.Commands;

public interface IDatabaseCommands
{
    Task SeedDatabaseAsync();
    Task RunMigrationsAsync();
    Task ResetDatabaseAsync();
    Task CheckDatabaseStatusAsync();
}