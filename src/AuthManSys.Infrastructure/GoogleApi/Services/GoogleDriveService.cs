using Google.Apis.Drive.v3;
using GoogleFile = Google.Apis.Drive.v3.Data.File;
using GooglePermission = Google.Apis.Drive.v3.Data.Permission;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AuthManSys.Infrastructure.GoogleApi.Authentication;
using AuthManSys.Infrastructure.GoogleApi.Configuration;

namespace AuthManSys.Infrastructure.GoogleApi.Services;

public class GoogleDriveService : IGoogleDriveService
{
    private readonly IGoogleAuthService _authService;
    private readonly GoogleApiSettings _settings;
    private readonly ILogger<GoogleDriveService> _logger;
    private DriveService? _driveService;

    public GoogleDriveService(
        IGoogleAuthService authService,
        IOptions<GoogleApiSettings> settings,
        ILogger<GoogleDriveService> logger)
    {
        _authService = authService;
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task<DriveService> GetDriveServiceAsync()
    {
        if (_driveService == null)
        {
            var initializer = _authService.GetServiceInitializer(_settings.ApplicationName);
            _driveService = new DriveService(initializer);
        }
        return _driveService;
    }

    public async Task<string> CreateDocumentAsync(string title, string? folderId = null)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();

            var fileMetadata = new GoogleFile()
            {
                Name = title,
                MimeType = "application/vnd.google-apps.document"
            };

            if (!string.IsNullOrEmpty(folderId))
            {
                fileMetadata.Parents = new List<string> { folderId };
            }
            else if (!string.IsNullOrEmpty(_settings.DefaultFolderId))
            {
                fileMetadata.Parents = new List<string> { _settings.DefaultFolderId };
            }

            var request = driveService.Files.Create(fileMetadata);
            request.Fields = "id, name, webViewLink, parents";

            var file = await request.ExecuteAsync();

            _logger.LogInformation("Created Google Document: {Title} with ID: {FileId}", title, file.Id);

            return file.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create Google Document: {Title}", title);
            throw;
        }
    }

    public async Task<GoogleFile> GetFileAsync(string fileId)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();
            var request = driveService.Files.Get(fileId);
            request.Fields = "id, name, mimeType, createdTime, modifiedTime, webViewLink, parents, owners, size";

            var file = await request.ExecuteAsync();

            _logger.LogDebug("Retrieved file information for ID: {FileId}", fileId);

            return file;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file with ID: {FileId}", fileId);
            throw;
        }
    }

    public async Task<string> ShareDocumentAsync(string fileId, string email, string role = "writer")
    {
        try
        {
            var driveService = await GetDriveServiceAsync();

            var permission = new GooglePermission()
            {
                Type = "user",
                Role = role,
                EmailAddress = email
            };

            var request = driveService.Permissions.Create(permission, fileId);
            request.SendNotificationEmail = true;
            request.EmailMessage = $"A document has been shared with you.";

            var result = await request.ExecuteAsync();

            _logger.LogInformation("Shared document {FileId} with {Email} as {Role}", fileId, email, role);

            return result.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to share document {FileId} with {Email}", fileId, email);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string fileId)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();
            await driveService.Files.Delete(fileId).ExecuteAsync();

            _logger.LogInformation("Deleted document with ID: {FileId}", fileId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document with ID: {FileId}", fileId);
            return false;
        }
    }

    public async Task<IList<GoogleFile>> ListFilesAsync(string? folderId = null, int maxResults = 10)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();
            var request = driveService.Files.List();

            var query = "mimeType='application/vnd.google-apps.document'";
            if (!string.IsNullOrEmpty(folderId))
            {
                query += $" and '{folderId}' in parents";
            }

            request.Q = query;
            request.PageSize = maxResults;
            request.Fields = "nextPageToken, files(id, name, createdTime, modifiedTime, webViewLink)";
            request.OrderBy = "modifiedTime desc";

            var result = await request.ExecuteAsync();

            _logger.LogDebug("Listed {Count} files", result.Files?.Count ?? 0);

            return result.Files ?? new List<GoogleFile>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list files");
            throw;
        }
    }

    public async Task<string> CreateFolderAsync(string name, string? parentFolderId = null)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();

            var folderMetadata = new GoogleFile()
            {
                Name = name,
                MimeType = "application/vnd.google-apps.folder"
            };

            if (!string.IsNullOrEmpty(parentFolderId))
            {
                folderMetadata.Parents = new List<string> { parentFolderId };
            }

            var request = driveService.Files.Create(folderMetadata);
            request.Fields = "id, name";

            var folder = await request.ExecuteAsync();

            _logger.LogInformation("Created folder: {Name} with ID: {FolderId}", name, folder.Id);

            return folder.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create folder: {Name}", name);
            throw;
        }
    }

    public async Task<string> MoveFileAsync(string fileId, string destinationFolderId)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();

            // First, get the file to retrieve current parents
            var file = await GetFileAsync(fileId);
            var previousParents = string.Join(",", file.Parents ?? new List<string>());

            var request = driveService.Files.Update(new GoogleFile(), fileId);
            request.AddParents = destinationFolderId;
            request.RemoveParents = previousParents;
            request.Fields = "id, parents";

            var updatedFile = await request.ExecuteAsync();

            _logger.LogInformation("Moved file {FileId} to folder {FolderId}", fileId, destinationFolderId);

            return updatedFile.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move file {FileId} to folder {FolderId}", fileId, destinationFolderId);
            throw;
        }
    }

    public async Task<bool> SetFilePermissionsAsync(string fileId, bool isPublic = false)
    {
        try
        {
            var driveService = await GetDriveServiceAsync();

            if (isPublic)
            {
                var publicPermission = new GooglePermission()
                {
                    Type = "anyone",
                    Role = "reader"
                };

                await driveService.Permissions.Create(publicPermission, fileId).ExecuteAsync();
                _logger.LogInformation("Set file {FileId} to public", fileId);
            }
            else
            {
                // List current permissions and remove public ones
                var permissions = await driveService.Permissions.List(fileId).ExecuteAsync();

                foreach (var permission in permissions.Permissions.Where(p => p.Type == "anyone"))
                {
                    await driveService.Permissions.Delete(fileId, permission.Id).ExecuteAsync();
                }

                _logger.LogInformation("Removed public access from file {FileId}", fileId);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set permissions for file {FileId}", fileId);
            return false;
        }
    }
}