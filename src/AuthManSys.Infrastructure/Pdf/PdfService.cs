using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Application.Common.Helpers;
using AuthManSys.Domain.Entities;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.Extensions.Logging;

namespace AuthManSys.Infrastructure.Pdf;

public class PdfService : IPdfService
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly ILogger<PdfService> _logger;

    public PdfService(IActivityLogRepository activityLogRepository, ILogger<PdfService> logger)
    {
        _activityLogRepository = activityLogRepository;
        _logger = logger;
    }

    public async Task GenerateUserActivityReportAsync(IEnumerable<UserActivityLog> activityLogs, string filePath)
    {
        try
        {
            _logger.LogInformation("Generating user activity PDF report at {FilePath}", filePath);

            // Create directory if it doesn't exist
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Add title
            document.Add(new Paragraph("User Activity Report")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            // Add generation timestamp
            document.Add(new Paragraph($"Generated on: {JamaicaTimeHelper.Now:yyyy-MM-dd HH:mm:ss} (Jamaica Time)")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginBottom(20));

            // Create table
            var table = new Table(6); // 6 columns
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Add table headers
            table.AddHeaderCell(CreateHeaderCell("Event Type"));
            table.AddHeaderCell(CreateHeaderCell("User ID"));
            table.AddHeaderCell(CreateHeaderCell("Description"));
            table.AddHeaderCell(CreateHeaderCell("Timestamp"));
            table.AddHeaderCell(CreateHeaderCell("IP Address"));
            table.AddHeaderCell(CreateHeaderCell("Device"));

            // Add data rows
            var logsList = activityLogs.ToList();

            if (!logsList.Any())
            {
                document.Add(new Paragraph("No activity logs found for the specified period.")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginTop(50));
            }
            else
            {
                foreach (var log in logsList.OrderByDescending(l => l.Timestamp))
                {
                    table.AddCell(CreateDataCell(log.EventType.ToString()));
                    table.AddCell(CreateDataCell(log.UserId?.ToString() ?? "N/A"));
                    table.AddCell(CreateDataCell(log.Description ?? "N/A"));
                    table.AddCell(CreateDataCell(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")));
                    table.AddCell(CreateDataCell(log.IPAddress ?? "N/A"));
                    table.AddCell(CreateDataCell(log.Device ?? "N/A"));
                }

                document.Add(table);

                // Add summary
                document.Add(new Paragraph($"\nTotal Activities: {logsList.Count}")
                    .SetFontSize(12)
                    .SetMarginTop(20));
            }

            _logger.LogInformation("Successfully generated PDF report with {LogCount} entries at {FilePath}",
                logsList.Count, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF report at {FilePath}", filePath);
            throw;
        }
    }

    public async Task GenerateAllUsersActivityReportAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Generating all users activity PDF report at {FilePath}", filePath);

            // Get all activity logs from the last 24 hours
            var fromDate = JamaicaTimeHelper.Now.AddDays(-1);

            // Get all activity logs using the new method
            var activityLogs = await _activityLogRepository.GetAllActivitiesAsync(
                fromDate: fromDate,
                toDate: null,
                pageNumber: 1,
                pageSize: 1000);

            await GenerateUserActivityReportAsync(activityLogs, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating all users activity PDF report at {FilePath}", filePath);
            throw;
        }
    }

    private static Cell CreateHeaderCell(string content)
    {
        return new Cell()
            .Add(new Paragraph(content).SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)))
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetPadding(8);
    }

    private static Cell CreateDataCell(string content)
    {
        return new Cell()
            .Add(new Paragraph(content))
            .SetTextAlignment(TextAlignment.LEFT)
            .SetPadding(6);
    }
}