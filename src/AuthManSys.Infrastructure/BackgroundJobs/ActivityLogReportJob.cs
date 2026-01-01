using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace AuthManSys.Infrastructure.BackgroundJobs;

public class ActivityLogReportJob : IActivityLogReportJob
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<ActivityLogReportJob> _logger;

    public ActivityLogReportJob(IPdfService pdfService, ILogger<ActivityLogReportJob> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    public async Task GenerateUserActivityReportAsync()
    {
        try
        {
            _logger.LogInformation("Starting background job: Generate User Activity Report");

            // Generate file name with timestamp
            var fileName = $"UserActivityReport_{JamaicaTimeHelper.Now:yyyyMMdd_HHmmss}.pdf";

            // Create reports directory if it doesn't exist
            var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "ActivityLogs");
            if (!Directory.Exists(reportsDirectory))
            {
                Directory.CreateDirectory(reportsDirectory);
            }

            var filePath = Path.Combine(reportsDirectory, fileName);

            // Generate the PDF report
            await _pdfService.GenerateAllUsersActivityReportAsync(filePath);

            _logger.LogInformation("Successfully completed background job: User Activity Report generated at {FilePath}", filePath);

            // Optional: Clean up old reports (keep only last 30 days)
            await CleanupOldReportsAsync(reportsDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background job: Generate User Activity Report");
            throw;
        }
    }

    private async Task CleanupOldReportsAsync(string reportsDirectory)
    {
        try
        {
            var cutoffDate = JamaicaTimeHelper.Now.AddDays(-30);
            var files = Directory.GetFiles(reportsDirectory, "UserActivityReport_*.pdf");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                    _logger.LogDebug("Deleted old report file: {FileName}", fileInfo.Name);
                }
            }

            _logger.LogInformation("Cleanup completed: Removed reports older than {CutoffDate}", cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during cleanup of old reports");
            // Don't throw - cleanup failure shouldn't fail the main job
        }

        await Task.CompletedTask;
    }
}