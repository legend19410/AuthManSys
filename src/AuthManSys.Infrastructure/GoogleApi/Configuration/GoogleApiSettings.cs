namespace AuthManSys.Infrastructure.GoogleApi.Configuration;

public class GoogleApiSettings
{
    public const string SectionName = "GoogleApi";

    public string ServiceAccountKeyPath { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = "AuthManSys";
    public string[] Scopes { get; set; } = Array.Empty<string>();
    public string DefaultFolderId { get; set; } = string.Empty;
    public bool EnableLogging { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
}

public static class GoogleApiScopes
{
    public const string DriveFile = "https://www.googleapis.com/auth/drive.file";
    public const string Drive = "https://www.googleapis.com/auth/drive";
    public const string DocsReadonly = "https://www.googleapis.com/auth/documents.readonly";
    public const string Docs = "https://www.googleapis.com/auth/documents";

    public static string[] DefaultScopes => new[]
    {
        Drive,
        Docs
    };
}