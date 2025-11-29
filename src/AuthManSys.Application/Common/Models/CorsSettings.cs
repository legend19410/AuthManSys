namespace AuthManSys.Application.Common.Models;

public class CorsSettings
{
    public string PolicyName { get; set; } = "DefaultCorsPolicy";
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
    public string[] ExposedHeaders { get; set; } = Array.Empty<string>();
    public bool AllowCredentials { get; set; } = false;
    public int PreflightMaxAge { get; set; } = 86400; // 24 hours in seconds
    public bool AllowAnyOrigin { get; set; } = false;
    public bool AllowAnyMethod { get; set; } = false;
    public bool AllowAnyHeader { get; set; } = false;
}