using Google.Apis.Docs.v1.Data;

namespace AuthManSys.Infrastructure.GoogleApi.Services;

public interface IGoogleDocsService
{
    Task<Document> GetDocumentAsync(string documentId);
    Task WriteTextAsync(string documentId, string text, int? insertIndex = null);
    Task AppendTextAsync(string documentId, string text);
    Task ReplaceTextAsync(string documentId, string oldText, string newText);
    Task InsertTableAsync(string documentId, int rows, int columns, int? insertIndex = null);
    Task InsertImageAsync(string documentId, string imageUrl, int? insertIndex = null);
    Task FormatTextAsync(string documentId, int startIndex, int endIndex, TextStyle textStyle);
    Task<string> ExportAsPlainTextAsync(string documentId);
    Task<string> ExportAsPdfAsync(string documentId);
    Task ClearDocumentAsync(string documentId);
    Task InsertPageBreakAsync(string documentId, int? insertIndex = null);
}

public class DocumentTextContent
{
    public string Text { get; set; } = string.Empty;
    public TextStyle? Style { get; set; }
    public int? InsertIndex { get; set; }
}

public class DocumentTableOptions
{
    public int Rows { get; set; } = 1;
    public int Columns { get; set; } = 1;
    public List<List<string>>? Data { get; set; }
    public int? InsertIndex { get; set; }
}