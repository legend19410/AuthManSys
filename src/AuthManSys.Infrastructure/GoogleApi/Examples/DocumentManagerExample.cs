using Microsoft.Extensions.Logging;
using AuthManSys.Infrastructure.GoogleApi.Services;
using Google.Apis.Docs.v1.Data;

namespace AuthManSys.Infrastructure.GoogleApi.Examples;

/// <summary>
/// Example class demonstrating how to use Google Docs and Drive services.
/// This class shows common patterns and best practices for document management.
/// </summary>
public class DocumentManagerExample
{
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDocsService _docsService;
    private readonly ILogger<DocumentManagerExample> _logger;

    public DocumentManagerExample(
        IGoogleDriveService driveService,
        IGoogleDocsService docsService,
        ILogger<DocumentManagerExample> logger)
    {
        _driveService = driveService;
        _docsService = docsService;
        _logger = logger;
    }

    /// <summary>
    /// Creates a comprehensive project report with multiple sections, formatting, and a table.
    /// </summary>
    public async Task<string> CreateProjectReportAsync(string projectName, string projectDescription)
    {
        try
        {
            // 1. Create the document
            var documentTitle = $"{projectName} - Project Report";
            var documentId = await _driveService.CreateDocumentAsync(documentTitle);

            _logger.LogInformation("Created document: {Title} with ID: {DocumentId}", documentTitle, documentId);

            // 2. Add title and format it
            await _docsService.WriteTextAsync(documentId, $"{documentTitle}\n\n");

            // Format title as bold and larger font
            var titleStyle = new TextStyle
            {
                Bold = true,
                FontSize = new Dimension { Magnitude = 18, Unit = "PT" }
            };
            await _docsService.FormatTextAsync(documentId, 0, documentTitle.Length, titleStyle);

            // 3. Add sections
            await AddExecutiveSummaryAsync(documentId, projectDescription);
            await AddProjectDetailsTableAsync(documentId);
            await AddMilestonesAsync(documentId);
            await AddConclusionAsync(documentId);

            return documentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create project report for {ProjectName}", projectName);
            throw;
        }
    }

    /// <summary>
    /// Demonstrates batch document creation and organization into folders.
    /// </summary>
    public async Task<List<string>> CreateDocumentBatchAsync(List<string> documentTitles, string folderName)
    {
        var documentIds = new List<string>();

        try
        {
            // Create a folder for organization
            var folderId = await _driveService.CreateFolderAsync(folderName);
            _logger.LogInformation("Created folder: {FolderName} with ID: {FolderId}", folderName, folderId);

            // Create documents in parallel for efficiency
            var creationTasks = documentTitles.Select(async title =>
            {
                var docId = await _driveService.CreateDocumentAsync(title, folderId);
                await _docsService.WriteTextAsync(docId, $"Document: {title}\n\nContent goes here...");
                return docId;
            });

            documentIds = (await Task.WhenAll(creationTasks)).ToList();

            _logger.LogInformation("Created {Count} documents in folder {FolderName}", documentIds.Count, folderName);
            return documentIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create document batch");
            throw;
        }
    }

    /// <summary>
    /// Demonstrates document sharing with different permission levels.
    /// </summary>
    public async Task ShareDocumentWithTeamAsync(string documentId, List<string> viewerEmails, List<string> editorEmails)
    {
        try
        {
            // Share with viewers
            foreach (var email in viewerEmails)
            {
                await _driveService.ShareDocumentAsync(documentId, email, "reader");
                _logger.LogDebug("Shared document {DocumentId} with {Email} as reader", documentId, email);
            }

            // Share with editors
            foreach (var email in editorEmails)
            {
                await _driveService.ShareDocumentAsync(documentId, email, "writer");
                _logger.LogDebug("Shared document {DocumentId} with {Email} as writer", documentId, email);
            }

            _logger.LogInformation("Shared document {DocumentId} with {ViewerCount} viewers and {EditorCount} editors",
                documentId, viewerEmails.Count, editorEmails.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to share document {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Updates a document with new content using replace operations.
    /// </summary>
    public async Task UpdateDocumentContentAsync(string documentId, Dictionary<string, string> replacements)
    {
        try
        {
            foreach (var replacement in replacements)
            {
                await _docsService.ReplaceTextAsync(documentId, replacement.Key, replacement.Value);
                _logger.LogDebug("Replaced '{OldText}' with '{NewText}' in document {DocumentId}",
                    replacement.Key, replacement.Value, documentId);
            }

            _logger.LogInformation("Updated document {DocumentId} with {Count} replacements",
                documentId, replacements.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update document content for {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Exports document content and cleans up if needed.
    /// </summary>
    public async Task<string> ExportAndCleanupAsync(string documentId, bool deleteAfterExport = false)
    {
        try
        {
            // Export content as plain text
            var content = await _docsService.ExportAsPlainTextAsync(documentId);
            _logger.LogInformation("Exported {Length} characters from document {DocumentId}",
                content.Length, documentId);

            // Optionally delete the document
            if (deleteAfterExport)
            {
                var deleted = await _driveService.DeleteDocumentAsync(documentId);
                if (deleted)
                {
                    _logger.LogInformation("Deleted document {DocumentId} after export", documentId);
                }
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export or cleanup document {DocumentId}", documentId);
            throw;
        }
    }

    #region Private Helper Methods

    private async Task AddExecutiveSummaryAsync(string documentId, string description)
    {
        await _docsService.AppendTextAsync(documentId, "Executive Summary\n");

        var headerStyle = new TextStyle
        {
            Bold = true,
            FontSize = new Dimension { Magnitude = 14, Unit = "PT" }
        };

        // Get current document length to apply formatting to header
        var doc = await _docsService.GetDocumentAsync(documentId);
        var currentLength = doc.Body.Content.Sum(c => c.EndIndex - c.StartIndex ?? 0);

        await _docsService.FormatTextAsync(documentId, currentLength - 17, currentLength - 1, headerStyle);
        await _docsService.AppendTextAsync(documentId, $"{description}\n\n");
    }

    private async Task AddProjectDetailsTableAsync(string documentId)
    {
        await _docsService.AppendTextAsync(documentId, "Project Details\n\n");
        await _docsService.InsertTableAsync(documentId, 4, 2);

        // Note: In a real implementation, you would populate the table cells
        // This requires more complex API calls to insert text into specific table cells
        await _docsService.AppendTextAsync(documentId, "\n\n");
    }

    private async Task AddMilestonesAsync(string documentId)
    {
        await _docsService.AppendTextAsync(documentId, "Project Milestones\n");
        await _docsService.AppendTextAsync(documentId, "• Phase 1: Requirements Gathering\n");
        await _docsService.AppendTextAsync(documentId, "• Phase 2: Design and Development\n");
        await _docsService.AppendTextAsync(documentId, "• Phase 3: Testing and Deployment\n\n");
    }

    private async Task AddConclusionAsync(string documentId)
    {
        await _docsService.AppendTextAsync(documentId, "Conclusion\n");
        await _docsService.AppendTextAsync(documentId,
            "This project report outlines the key aspects and timeline for successful completion.\n\n");
    }

    #endregion
}