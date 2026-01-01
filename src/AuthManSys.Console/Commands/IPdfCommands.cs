namespace AuthManSys.Console.Commands;

public interface IPdfCommands
{
    Task TestPdfGenerationAsync();
    Task GenerateTestReportAsync();
}