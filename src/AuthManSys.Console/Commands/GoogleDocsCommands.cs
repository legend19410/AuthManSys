using AuthManSys.Infrastructure.GoogleApi.Services;
using Google.Apis.Docs.v1.Data;

namespace AuthManSys.Console.Commands;

public class GoogleDocsCommands : IGoogleDocsCommands
{
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDocsService _docsService;

    public GoogleDocsCommands(
        IGoogleDriveService driveService,
        IGoogleDocsService docsService)
    {
        _driveService = driveService;
        _docsService = docsService;
    }

    public async Task CreateDocumentAsync(string title)
    {
        try
        {
            System.Console.WriteLine($"📄 Creating Google Document: '{title}'...");

            var documentId = await _driveService.CreateDocumentAsync(title);
            var file = await _driveService.GetFileAsync(documentId);

            System.Console.WriteLine("✅ Document created successfully!");
            System.Console.WriteLine($"   📌 Document ID: {documentId}");
            System.Console.WriteLine($"   📝 Title: {file.Name}");
            System.Console.WriteLine($"   🔗 View Link: {file.WebViewLink}");
            System.Console.WriteLine($"   📅 Created: {file.CreatedTime:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error creating document: {ex.Message}");
        }
    }

    public async Task WriteToDocumentAsync(string documentId, string content)
    {
        try
        {
            System.Console.WriteLine($"✏️  Writing content to document {documentId}...");

            await _docsService.AppendTextAsync(documentId, content + "\n");

            System.Console.WriteLine("✅ Content written successfully!");
            System.Console.WriteLine($"   📝 Added {content.Length} characters");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error writing to document: {ex.Message}");
        }
    }

    public async Task CreateAndWriteAsync(string title, string content)
    {
        try
        {
            System.Console.WriteLine($"📄 Creating document '{title}' with content...");

            // Create document
            var documentId = await _driveService.CreateDocumentAsync(title);

            // Write content
            await _docsService.WriteTextAsync(documentId, content);

            // Get final document info
            var file = await _driveService.GetFileAsync(documentId);

            System.Console.WriteLine("✅ Document created and content written successfully!");
            System.Console.WriteLine($"   📌 Document ID: {documentId}");
            System.Console.WriteLine($"   📝 Title: {file.Name}");
            System.Console.WriteLine($"   📊 Content Length: {content.Length} characters");
            System.Console.WriteLine($"   🔗 View Link: {file.WebViewLink}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error creating document with content: {ex.Message}");
        }
    }

    public async Task ListDocumentsAsync()
    {
        try
        {
            System.Console.WriteLine("📋 Listing Google Documents...");

            var files = await _driveService.ListFilesAsync(maxResults: 20);

            if (!files.Any())
            {
                System.Console.WriteLine("   📭 No documents found.");
                return;
            }

            System.Console.WriteLine($"   📄 Found {files.Count} document(s):");
            System.Console.WriteLine();

            foreach (var file in files)
            {
                System.Console.WriteLine($"   📌 {file.Name}");
                System.Console.WriteLine($"      ID: {file.Id}");
                System.Console.WriteLine($"      Modified: {file.ModifiedTime:yyyy-MM-dd HH:mm:ss}");
                System.Console.WriteLine($"      Link: {file.WebViewLink}");
                System.Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error listing documents: {ex.Message}");
        }
    }

    public async Task GetDocumentInfoAsync(string documentId)
    {
        try
        {
            System.Console.WriteLine($"📄 Getting document information for ID: {documentId}...");

            var file = await _driveService.GetFileAsync(documentId);
            var document = await _docsService.GetDocumentAsync(documentId);

            System.Console.WriteLine("✅ Document information:");
            System.Console.WriteLine($"   📌 Title: {file.Name}");
            System.Console.WriteLine($"   📝 Document ID: {file.Id}");
            System.Console.WriteLine($"   📅 Created: {file.CreatedTime:yyyy-MM-dd HH:mm:ss}");
            System.Console.WriteLine($"   🔄 Modified: {file.ModifiedTime:yyyy-MM-dd HH:mm:ss}");
            System.Console.WriteLine($"   👤 Owner: {file.Owners?.FirstOrDefault()?.DisplayName ?? "Unknown"}");
            System.Console.WriteLine($"   🔗 View Link: {file.WebViewLink}");

            // Get content length
            var contentLength = document.Body?.Content?.Sum(c =>
                c.Paragraph?.Elements?.Sum(e => e.TextRun?.Content?.Length ?? 0) ?? 0) ?? 0;
            System.Console.WriteLine($"   📊 Content Length: {contentLength} characters");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error getting document info: {ex.Message}");
        }
    }

    public async Task ShareDocumentAsync(string documentId, string email, string role = "reader")
    {
        try
        {
            System.Console.WriteLine($"🔗 Sharing document {documentId} with {email} as {role}...");

            var permissionId = await _driveService.ShareDocumentAsync(documentId, email, role);

            System.Console.WriteLine("✅ Document shared successfully!");
            System.Console.WriteLine($"   👤 Shared with: {email}");
            System.Console.WriteLine($"   🔐 Role: {role}");
            System.Console.WriteLine($"   📌 Permission ID: {permissionId}");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error sharing document: {ex.Message}");
        }
    }

    public async Task ExportDocumentAsync(string documentId, string format = "text")
    {
        try
        {
            System.Console.WriteLine($"📤 Exporting document {documentId} as {format}...");

            switch (format.ToLower())
            {
                case "text":
                case "txt":
                    var textContent = await _docsService.ExportAsPlainTextAsync(documentId);
                    System.Console.WriteLine("✅ Document exported as plain text:");
                    System.Console.WriteLine("" + new string('=', 50));
                    System.Console.WriteLine(textContent);
                    System.Console.WriteLine("" + new string('=', 50));
                    System.Console.WriteLine($"   📊 Total characters: {textContent.Length}");
                    break;

                case "pdf":
                    var pdfUrl = await _docsService.ExportAsPdfAsync(documentId);
                    System.Console.WriteLine("✅ PDF export URL generated:");
                    System.Console.WriteLine($"   🔗 PDF URL: {pdfUrl}");
                    System.Console.WriteLine("   💡 Open this URL in a browser to download the PDF");
                    break;

                default:
                    System.Console.WriteLine($"❌ Unsupported format '{format}'. Supported: text, pdf");
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error exporting document: {ex.Message}");
        }
    }
}