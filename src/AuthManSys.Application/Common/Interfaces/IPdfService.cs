using AuthManSys.Domain.Entities;

namespace AuthManSys.Application.Common.Interfaces;

public interface IPdfService
{
    /// <summary>
    /// Generates a PDF report of user activity logs
    /// </summary>
    /// <param name="activityLogs">Collection of user activity logs to include in the PDF</param>
    /// <param name="filePath">Full path where the PDF file should be saved</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task GenerateUserActivityReportAsync(IEnumerable<UserActivityLog> activityLogs, string filePath);

    /// <summary>
    /// Generates a PDF report for all user activities
    /// </summary>
    /// <param name="filePath">Full path where the PDF file should be saved</param>
    /// <returns>Task representing the asynchronous operation</returns>
    Task GenerateAllUsersActivityReportAsync(string filePath);
}