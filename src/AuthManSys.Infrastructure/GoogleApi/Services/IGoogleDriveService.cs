using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace AuthManSys.Infrastructure.GoogleApi.Services;

public interface IGoogleDriveService
{
    Task<string> CreateDocumentAsync(string title, string? folderId = null);
    Task<GoogleFile> GetFileAsync(string fileId);
    Task<string> ShareDocumentAsync(string fileId, string email, string role = "writer");
    Task<bool> DeleteDocumentAsync(string fileId);
    Task<IList<GoogleFile>> ListFilesAsync(string? folderId = null, int maxResults = 10);
    Task<string> CreateFolderAsync(string name, string? parentFolderId = null);
    Task<string> MoveFileAsync(string fileId, string destinationFolderId);
    Task<bool> SetFilePermissionsAsync(string fileId, bool isPublic = false);
}

public class DocumentCreationOptions
{
    public string Title { get; set; } = string.Empty;
    public string? FolderId { get; set; }
    public string? Description { get; set; }
    public List<string>? SharedEmails { get; set; }
    public bool IsPublic { get; set; } = false;
}