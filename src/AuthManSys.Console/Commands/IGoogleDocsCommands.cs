namespace AuthManSys.Console.Commands;

public interface IGoogleDocsCommands
{
    Task CreateDocumentAsync(string title);
    Task WriteToDocumentAsync(string documentId, string content);
    Task CreateAndWriteAsync(string title, string content);
    Task ListDocumentsAsync();
    Task GetDocumentInfoAsync(string documentId);
    Task ShareDocumentAsync(string documentId, string email, string role = "reader");
    Task ExportDocumentAsync(string documentId, string format = "text");
}