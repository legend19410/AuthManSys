using Google.Apis.Docs.v1;
using Google.Apis.Docs.v1.Data;
using GoogleRange = Google.Apis.Docs.v1.Data.Range;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AuthManSys.Infrastructure.GoogleApi.Authentication;
using AuthManSys.Infrastructure.GoogleApi.Configuration;

namespace AuthManSys.Infrastructure.GoogleApi.Services;

public class GoogleDocsService : IGoogleDocsService
{
    private readonly IGoogleAuthService _authService;
    private readonly GoogleApiSettings _settings;
    private readonly ILogger<GoogleDocsService> _logger;
    private DocsService? _docsService;

    public GoogleDocsService(
        IGoogleAuthService authService,
        IOptions<GoogleApiSettings> settings,
        ILogger<GoogleDocsService> logger)
    {
        _authService = authService;
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task<DocsService> GetDocsServiceAsync()
    {
        if (_docsService == null)
        {
            var initializer = _authService.GetServiceInitializer(_settings.ApplicationName);
            _docsService = new DocsService(initializer);
        }
        return _docsService;
    }

    public async Task<Document> GetDocumentAsync(string documentId)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();
            var request = docsService.Documents.Get(documentId);
            var document = await request.ExecuteAsync();

            _logger.LogDebug("Retrieved document: {DocumentId}", documentId);

            return document;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task WriteTextAsync(string documentId, string text, int? insertIndex = null)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();

            var requests = new List<Request>();

            if (insertIndex.HasValue)
            {
                requests.Add(new Request
                {
                    InsertText = new InsertTextRequest
                    {
                        Location = new Location { Index = insertIndex.Value },
                        Text = text
                    }
                });
            }
            else
            {
                // Get document to find the end index
                var document = await GetDocumentAsync(documentId);
                var endIndex = document.Body.Content.Last().EndIndex.Value - 1;

                requests.Add(new Request
                {
                    InsertText = new InsertTextRequest
                    {
                        Location = new Location { Index = endIndex },
                        Text = text
                    }
                });
            }

            var batchUpdateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requests
            };

            await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

            _logger.LogInformation("Wrote text to document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write text to document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task AppendTextAsync(string documentId, string text)
    {
        try
        {
            var document = await GetDocumentAsync(documentId);
            var endIndex = document.Body.Content.Last().EndIndex.Value - 1;

            await WriteTextAsync(documentId, text, endIndex);

            _logger.LogDebug("Appended text to document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to append text to document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task ReplaceTextAsync(string documentId, string oldText, string newText)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();

            var requests = new List<Request>
            {
                new Request
                {
                    ReplaceAllText = new ReplaceAllTextRequest
                    {
                        ContainsText = new SubstringMatchCriteria
                        {
                            Text = oldText,
                            MatchCase = false
                        },
                        ReplaceText = newText
                    }
                }
            };

            var batchUpdateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requests
            };

            await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

            _logger.LogInformation("Replaced text in document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to replace text in document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task InsertTableAsync(string documentId, int rows, int columns, int? insertIndex = null)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();

            int targetIndex;
            if (insertIndex.HasValue)
            {
                targetIndex = insertIndex.Value;
            }
            else
            {
                var document = await GetDocumentAsync(documentId);
                targetIndex = document.Body.Content.Last().EndIndex.Value - 1;
            }

            var requests = new List<Request>
            {
                new Request
                {
                    InsertTable = new InsertTableRequest
                    {
                        Location = new Location { Index = targetIndex },
                        Rows = rows,
                        Columns = columns
                    }
                }
            };

            var batchUpdateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requests
            };

            await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

            _logger.LogInformation("Inserted table ({Rows}x{Columns}) in document: {DocumentId}", rows, columns, documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert table in document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task InsertImageAsync(string documentId, string imageUrl, int? insertIndex = null)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();

            int targetIndex;
            if (insertIndex.HasValue)
            {
                targetIndex = insertIndex.Value;
            }
            else
            {
                var document = await GetDocumentAsync(documentId);
                targetIndex = document.Body.Content.Last().EndIndex.Value - 1;
            }

            var requests = new List<Request>
            {
                new Request
                {
                    InsertInlineImage = new InsertInlineImageRequest
                    {
                        Location = new Location { Index = targetIndex },
                        Uri = imageUrl
                    }
                }
            };

            var batchUpdateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requests
            };

            await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

            _logger.LogInformation("Inserted image in document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert image in document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task FormatTextAsync(string documentId, int startIndex, int endIndex, TextStyle textStyle)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();

            var requests = new List<Request>
            {
                new Request
                {
                    UpdateTextStyle = new UpdateTextStyleRequest
                    {
                        Range = new GoogleRange
                        {
                            StartIndex = startIndex,
                            EndIndex = endIndex
                        },
                        TextStyle = textStyle,
                        Fields = "*"
                    }
                }
            };

            var batchUpdateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requests
            };

            await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

            _logger.LogDebug("Applied text formatting to document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to format text in document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<string> ExportAsPlainTextAsync(string documentId)
    {
        try
        {
            var document = await GetDocumentAsync(documentId);
            var plainText = ExtractTextFromDocument(document);

            _logger.LogDebug("Exported document as plain text: {DocumentId}", documentId);

            return plainText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export document as plain text: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task<string> ExportAsPdfAsync(string documentId)
    {
        try
        {
            // Note: This would require Drive API to export as PDF
            // For now, returning the web view link
            var document = await GetDocumentAsync(documentId);
            var pdfUrl = $"https://docs.google.com/document/d/{documentId}/export?format=pdf";

            _logger.LogInformation("Generated PDF export URL for document: {DocumentId}", documentId);

            return pdfUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export document as PDF: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task ClearDocumentAsync(string documentId)
    {
        try
        {
            var document = await GetDocumentAsync(documentId);
            var docsService = await GetDocsServiceAsync();

            // Get the content length
            var endIndex = document.Body.Content.Last().EndIndex.Value - 1;

            if (endIndex > 1) // Don't delete if document is already empty
            {
                var requests = new List<Request>
                {
                    new Request
                    {
                        DeleteContentRange = new DeleteContentRangeRequest
                        {
                            Range = new GoogleRange
                            {
                                StartIndex = 1, // Start after the initial content
                                EndIndex = endIndex
                            }
                        }
                    }
                };

                var batchUpdateRequest = new BatchUpdateDocumentRequest
                {
                    Requests = requests
                };

                await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

                _logger.LogInformation("Cleared content from document: {DocumentId}", documentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear document: {DocumentId}", documentId);
            throw;
        }
    }

    public async Task InsertPageBreakAsync(string documentId, int? insertIndex = null)
    {
        try
        {
            var docsService = await GetDocsServiceAsync();

            int targetIndex;
            if (insertIndex.HasValue)
            {
                targetIndex = insertIndex.Value;
            }
            else
            {
                var document = await GetDocumentAsync(documentId);
                targetIndex = document.Body.Content.Last().EndIndex.Value - 1;
            }

            var requests = new List<Request>
            {
                new Request
                {
                    InsertPageBreak = new InsertPageBreakRequest
                    {
                        Location = new Location { Index = targetIndex }
                    }
                }
            };

            var batchUpdateRequest = new BatchUpdateDocumentRequest
            {
                Requests = requests
            };

            await docsService.Documents.BatchUpdate(batchUpdateRequest, documentId).ExecuteAsync();

            _logger.LogDebug("Inserted page break in document: {DocumentId}", documentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to insert page break in document: {DocumentId}", documentId);
            throw;
        }
    }

    private string ExtractTextFromDocument(Document document)
    {
        var text = string.Empty;

        if (document.Body?.Content != null)
        {
            foreach (var element in document.Body.Content)
            {
                if (element.Paragraph?.Elements != null)
                {
                    foreach (var paragraphElement in element.Paragraph.Elements)
                    {
                        if (!string.IsNullOrEmpty(paragraphElement.TextRun?.Content))
                        {
                            text += paragraphElement.TextRun.Content;
                        }
                    }
                }
            }
        }

        return text;
    }
}