# Google Docs & Drive API Integration

## Overview

The AuthManSys Infrastructure project includes comprehensive Google Docs and Drive API integration, allowing you to programmatically create, manage, and edit Google Documents and Drive files.

## Features

- **Document Creation**: Create new Google Documents programmatically
- **Content Management**: Write, append, replace, and format text in documents
- **Drive Operations**: Manage files and folders in Google Drive
- **Sharing & Permissions**: Share documents and manage access permissions
- **Authentication**: Service account-based authentication for server applications

## Setup and Configuration

### 1. Google Cloud Console Setup

1. Go to the [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Enable the following APIs:
   - Google Drive API
   - Google Docs API
4. Create a Service Account:
   - Go to IAM & Admin > Service Accounts
   - Click "Create Service Account"
   - Provide a name and description
   - Download the JSON key file
5. (Optional) Enable domain-wide delegation if needed

### 2. Application Configuration

Add the following configuration to your `appsettings.json`:

```json
{
  "GoogleApi": {
    "ServiceAccountKeyPath": "path/to/your/service-account-key.json",
    "ApplicationName": "AuthManSys",
    "Scopes": [
      "https://www.googleapis.com/auth/drive",
      "https://www.googleapis.com/auth/documents"
    ],
    "DefaultFolderId": "your-default-folder-id",
    "EnableLogging": true,
    "TimeoutSeconds": 30
  }
}
```

### 3. Dependency Injection

The Google API services are automatically registered when you call `AddInfrastructureServices()`. They can be injected into your controllers or services:

```csharp
public class DocumentController : ControllerBase
{
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDocsService _docsService;

    public DocumentController(
        IGoogleDriveService driveService,
        IGoogleDocsService docsService)
    {
        _driveService = driveService;
        _docsService = docsService;
    }
}
```

## Usage Examples

### Creating a Document

```csharp
// Create a new document
string documentId = await _driveService.CreateDocumentAsync("My New Document");

// Create in a specific folder
string documentId = await _driveService.CreateDocumentAsync(
    "Project Report",
    folderId: "your-folder-id"
);
```

### Writing Content

```csharp
// Write text to the document
await _docsService.WriteTextAsync(documentId, "Hello, World!");

// Append text
await _docsService.AppendTextAsync(documentId, "\nThis is additional content.");

// Insert text at specific position
await _docsService.WriteTextAsync(documentId, "Inserted text", insertIndex: 10);

// Replace text
await _docsService.ReplaceTextAsync(documentId, "old text", "new text");
```

### Formatting Text

```csharp
// Apply formatting to specific text range
var textStyle = new TextStyle
{
    Bold = true,
    FontSize = new Dimension { Magnitude = 14, Unit = "PT" },
    ForegroundColor = new OptionalColor
    {
        Color = new Color { RgbColor = new RgbColor { Red = 1.0f } }
    }
};

await _docsService.FormatTextAsync(documentId, startIndex: 0, endIndex: 10, textStyle);
```

### Adding Tables and Images

```csharp
// Insert a table
await _docsService.InsertTableAsync(documentId, rows: 3, columns: 4);

// Insert an image
await _docsService.InsertImageAsync(documentId, "https://example.com/image.jpg");

// Insert page break
await _docsService.InsertPageBreakAsync(documentId);
```

### Document Management

```csharp
// Get document information
var file = await _driveService.GetFileAsync(documentId);

// Share document
await _driveService.ShareDocumentAsync(
    documentId,
    "user@example.com",
    role: "writer"
);

// List documents
var files = await _driveService.ListFilesAsync(maxResults: 20);

// Delete document
await _driveService.DeleteDocumentAsync(documentId);
```

### Advanced Operations

```csharp
// Create folder
string folderId = await _driveService.CreateFolderAsync("Project Documents");

// Move document to folder
await _driveService.MoveFileAsync(documentId, folderId);

// Set public permissions
await _driveService.SetFilePermissionsAsync(documentId, isPublic: true);

// Export as plain text
string content = await _docsService.ExportAsPlainTextAsync(documentId);

// Get PDF export URL
string pdfUrl = await _docsService.ExportAsPdfAsync(documentId);

// Clear document content
await _docsService.ClearDocumentAsync(documentId);
```

## Complete Example: Document Generator

```csharp
[ApiController]
[Route("api/[controller]")]
public class DocumentGeneratorController : ControllerBase
{
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDocsService _docsService;

    public DocumentGeneratorController(
        IGoogleDriveService driveService,
        IGoogleDocsService docsService)
    {
        _driveService = driveService;
        _docsService = docsService;
    }

    [HttpPost("create-report")]
    public async Task<IActionResult> CreateReport([FromBody] ReportRequest request)
    {
        try
        {
            // Create the document
            var documentId = await _driveService.CreateDocumentAsync(request.Title);

            // Add title
            await _docsService.WriteTextAsync(documentId, $"{request.Title}\n\n");

            // Add introduction
            await _docsService.AppendTextAsync(documentId,
                "Executive Summary\n" +
                "This report provides an overview of the current status.\n\n");

            // Add a table for data
            await _docsService.InsertTableAsync(documentId, 3, 2);

            // Add conclusion
            await _docsService.AppendTextAsync(documentId, "\n\nConclusion\n");
            await _docsService.AppendTextAsync(documentId, request.Conclusion);

            // Share with stakeholders
            foreach (var email in request.ShareWithEmails)
            {
                await _driveService.ShareDocumentAsync(documentId, email, "reader");
            }

            // Get the document details
            var file = await _driveService.GetFileAsync(documentId);

            return Ok(new
            {
                DocumentId = documentId,
                Name = file.Name,
                WebViewLink = file.WebViewLink,
                CreatedTime = file.CreatedTime
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Error creating document: {ex.Message}");
        }
    }
}

public class ReportRequest
{
    public string Title { get; set; } = string.Empty;
    public string Conclusion { get; set; } = string.Empty;
    public List<string> ShareWithEmails { get; set; } = new();
}
```

## Error Handling

The services include comprehensive error handling and logging. Common errors include:

- **Authentication errors**: Invalid service account credentials
- **Permission errors**: Insufficient API permissions
- **Rate limiting**: Too many API requests
- **File not found**: Invalid document or file IDs

Example error handling:

```csharp
try
{
    await _docsService.WriteTextAsync(documentId, content);
}
catch (GoogleApiException ex)
{
    _logger.LogError(ex, "Google API error: {Error}", ex.Message);
    // Handle specific Google API errors
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error writing to document");
    throw;
}
```

## Security Considerations

1. **Service Account Security**: Keep your service account JSON file secure and never commit it to version control
2. **Least Privilege**: Only request the minimum required API scopes
3. **Access Control**: Implement proper authorization in your application endpoints
4. **Audit Logging**: Monitor and log all document operations
5. **Rate Limiting**: Implement rate limiting to avoid hitting API quotas

## API Limitations

- **Quota Limits**: Google APIs have daily and per-minute quotas
- **File Size**: Large documents may hit size limitations
- **Concurrent Operations**: Be mindful of concurrent access to the same document
- **Real-time Collaboration**: This API doesn't support real-time collaborative editing

## Testing

Use the validation method to test your configuration:

```csharp
var authService = serviceProvider.GetRequiredService<IGoogleAuthService>();
bool isValid = await authService.ValidateCredentialsAsync();

if (!isValid)
{
    throw new InvalidOperationException("Google API credentials are not valid");
}
```

## Troubleshooting

### Common Issues

1. **"Service account key file not found"**
   - Ensure the path in configuration is correct
   - Verify the file exists and is accessible

2. **"Access denied" errors**
   - Check API permissions in Google Cloud Console
   - Verify service account has necessary scopes

3. **"Quota exceeded" errors**
   - Monitor your API usage in Google Cloud Console
   - Implement exponential backoff for retries

4. **"Document not found" errors**
   - Verify document IDs are correct
   - Ensure the service account has access to the document

For more detailed troubleshooting, enable logging by setting `EnableLogging: true` in your configuration.