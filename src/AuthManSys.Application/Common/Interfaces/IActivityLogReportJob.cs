namespace AuthManSys.Application.Common.Interfaces;

public interface IActivityLogReportJob
{
    /// <summary>
    /// Generates a PDF report of all user activities and saves it to the file system
    /// This method is designed to be called by Hangfire as a background job
    /// </summary>
    /// <returns>Task representing the asynchronous operation</returns>
    Task GenerateUserActivityReportAsync();
}