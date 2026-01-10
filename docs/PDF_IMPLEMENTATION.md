# PDF Implementation Guide

This document provides comprehensive documentation for the PDF generation implementation in AuthManSys, covering architecture, components, usage, and maintenance.

## Overview

The AuthManSys PDF implementation provides automated PDF report generation capabilities, primarily focused on user activity reporting. The system uses **iText7** as the core PDF library and integrates with **Hangfire** for background job processing.

## Architecture

### Core Components

```
├── Application Layer
│   ├── Interfaces/
│   │   ├── IPdfService.cs                    # PDF service interface
│   │   └── IActivityLogReportJob.cs          # Background job interface
│   └── Common/Helpers/
│       └── JamaicaTimeHelper.cs              # Timezone utilities
│
├── Infrastructure Layer
│   ├── Pdf/
│   │   └── PdfService.cs                     # Core PDF generation service
│   ├── BackgroundJobs/
│   │   └── ActivityLogReportJob.cs           # Hangfire background job
│   └── DependencyInjection/
│       └── ServiceCollectionExtensions.cs   # DI configuration
│
└── Console Application
    ├── Commands/
    │   ├── IPdfCommands.cs                   # Console command interface
    │   └── PdfCommands.cs                    # Console command implementation
    └── Program.cs                            # Command registration
```

## Dependencies

### NuGet Packages

The PDF implementation relies on the following packages (defined in `AuthManSys.Infrastructure.csproj`):

```xml
<PackageReference Include="itext7" Version="9.4.0" />
<PackageReference Include="itext7.bouncy-castle-adapter" Version="9.4.0" />
```

### Internal Dependencies

- **IActivityLogRepository**: Data access for activity logs
- **JamaicaTimeHelper**: Timezone-aware timestamp handling
- **Hangfire**: Background job scheduling and processing
- **Microsoft.Extensions.Logging**: Comprehensive logging support

## Core Implementation

### 1. IPdfService Interface

**Location**: `src/AuthManSys.Application/Common/Interfaces/IPdfService.cs`

```csharp
public interface IPdfService
{
    /// <summary>
    /// Generates a PDF report of user activity logs
    /// </summary>
    Task GenerateUserActivityReportAsync(IEnumerable<UserActivityLog> activityLogs, string filePath);

    /// <summary>
    /// Generates a PDF report for all user activities
    /// </summary>
    Task GenerateAllUsersActivityReportAsync(string filePath);
}
```

### 2. PdfService Implementation

**Location**: `src/AuthManSys.Infrastructure/Pdf/PdfService.cs`

#### Key Features:
- **iText7 Integration**: Professional PDF generation with tables, headers, and styling
- **Activity Log Processing**: Comprehensive reporting of user activities
- **Timezone Awareness**: Uses Jamaica Time for all timestamps
- **Directory Management**: Automatically creates output directories
- **Error Handling**: Comprehensive exception handling with logging
- **Data Formatting**: Professional table layouts with headers and data cells

#### Core Methods:

##### GenerateUserActivityReportAsync
- Accepts a collection of activity logs and generates a formatted PDF
- Creates tabular reports with: Event Type, User ID, Description, Timestamp, IP Address, Device
- Includes report generation timestamp and summary statistics
- Handles empty data sets gracefully

##### GenerateAllUsersActivityReportAsync
- Retrieves activity logs from the last 24 hours
- Generates comprehensive reports for system administrators
- Uses pagination to handle large datasets (up to 1000 records)

#### PDF Structure:
1. **Header**: "User Activity Report" title
2. **Metadata**: Generation timestamp (Jamaica Time)
3. **Table**: Six-column activity data table
4. **Summary**: Total activity count
5. **Styling**: Professional formatting with gray headers and proper spacing

### 3. Background Job Implementation

**Location**: `src/AuthManSys.Infrastructure/BackgroundJobs/ActivityLogReportJob.cs`

#### Features:
- **Automated Scheduling**: Runs every minute via Hangfire cron jobs
- **File Management**: Timestamped file naming (`UserActivityReport_YYYYMMDD_HHMMSS.pdf`)
- **Directory Organization**: Saves to `Reports/ActivityLogs/` directory
- **Cleanup Process**: Automatically removes reports older than 30 days
- **Error Recovery**: Graceful error handling that doesn't affect subsequent runs

#### File Naming Convention:
```
UserActivityReport_20260102_105900.pdf
                  │        │
                  │        └── Time (HH:mm:ss)
                  └─────────── Date (YYYY:MM:DD)
```

#### Cleanup Strategy:
- Runs after each report generation
- Removes files older than 30 days
- Logs cleanup operations
- Non-blocking (cleanup failures don't affect report generation)

### 4. Console Commands

**Location**: `src/AuthManSys.Console/Commands/PdfCommands.cs`

#### Available Commands:

##### Test PDF Generation
```bash
dotnet run --project src/AuthManSys.Console -- pdf test
```
- Creates sample activity logs
- Generates test PDF report
- Validates file creation and provides feedback
- Useful for development and troubleshooting

##### Generate Test Report
```bash
dotnet run --project src/AuthManSys.Console -- pdf report
```
- Creates diverse, realistic test data (20 varied activity logs)
- Generates comprehensive activity report
- Shows detailed file information and statistics
- Demonstrates full system capabilities

#### Test Data Generation:
- **Diverse Events**: All activity event types
- **Multiple Users**: Including anonymous activities
- **Varied Sources**: Different IP addresses, devices, platforms, locations
- **Realistic Metadata**: Timestamps, random values, source tracking

## Configuration

### Dependency Injection Setup

**Location**: `src/AuthManSys.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`

```csharp
services.AddScoped<IPdfService, PdfService>();
services.AddScoped<IActivityLogReportJob, ActivityLogReportJob>();
```

### Background Job Scheduling

**Location**: `src/AuthManSys.Api/Program.cs`

```csharp
recurringJobManager.AddOrUpdate<IActivityLogReportJob>(
    "user-activity-report",
    job => job.GenerateUserActivityReportAsync(),
    Cron.Minutely);
```

### File System Integration

**Docker Volume Mapping**: `docker-compose.yml`
```yaml
volumes:
  - ./Reports:/src/Reports  # Mount Reports directory for PDF access
```

## File Organization

### Report Storage Structure
```
Reports/
├── ActivityLogs/                    # Automated background job reports
│   ├── UserActivityReport_20260102_105900.pdf
│   ├── UserActivityReport_20260102_104913.pdf
│   └── ...
└── Test/                           # Manual test reports
    ├── TestReport_20260102_110000.pdf
    └── ActivityReport_20260102_110500.pdf
```

### Directory Management
- **Automatic Creation**: Directories are created automatically if they don't exist
- **Docker Integration**: Reports persist outside containers via volume mounts
- **Access Control**: Files are created with appropriate permissions for web access

## Usage Examples

### 1. Manual PDF Generation (Console)
```bash
# Test basic PDF functionality
dotnet run --project src/AuthManSys.Console -- pdf test

# Generate comprehensive test report
dotnet run --project src/AuthManSys.Console -- pdf report
```

### 2. Programmatic Usage (Service)
```csharp
// Generate report for specific activities
var activities = await activityLogRepository.GetUserActivitiesAsync(userId, fromDate, toDate);
await pdfService.GenerateUserActivityReportAsync(activities, "custom-report.pdf");

// Generate system-wide report
await pdfService.GenerateAllUsersActivityReportAsync("all-users-report.pdf");
```

### 3. Background Job Integration
```csharp
// Manual job trigger (for testing)
var jobManager = serviceProvider.GetService<IRecurringJobManager>();
jobManager.AddOrUpdate<IActivityLogReportJob>(
    "manual-report",
    job => job.GenerateUserActivityReportAsync(),
    Cron.Never);

// Trigger immediate execution
BackgroundJob.Enqueue<IActivityLogReportJob>(job => job.GenerateUserActivityReportAsync());
```

## Monitoring and Troubleshooting

### Logging

The PDF system provides comprehensive logging at multiple levels:

```csharp
// PdfService logging
_logger.LogInformation("Generating user activity PDF report at {FilePath}", filePath);
_logger.LogInformation("Successfully generated PDF report with {LogCount} entries", logCount);
_logger.LogError(ex, "Error generating PDF report at {FilePath}", filePath);

// Background job logging
_logger.LogInformation("Starting background job: Generate User Activity Report");
_logger.LogInformation("Successfully completed background job: User Activity Report generated at {FilePath}", filePath);
_logger.LogError(ex, "Error in background job: Generate User Activity Report");
```

### Common Issues and Solutions

#### 1. File Access Errors
**Problem**: PDF generation fails with file access exceptions
**Solution**:
- Ensure output directory exists and is writable
- Check file permissions in Docker containers
- Verify volume mounts in docker-compose.yml

#### 2. Empty Reports
**Problem**: PDF contains "No activity logs found"
**Solution**:
- Verify activity logging is functioning
- Check date range filters (last 24 hours default)
- Confirm database connectivity

#### 3. Background Job Failures
**Problem**: Scheduled reports not generating
**Solution**:
- Check Hangfire dashboard at `/hangfire` (development only)
- Verify Hangfire configuration in ServiceCollectionExtensions.cs
- Review job scheduling in Program.cs

#### 4. Memory Issues
**Problem**: Large datasets causing memory problems
**Solution**:
- Implement pagination in data retrieval
- Consider streaming PDF generation for very large reports
- Monitor report size and optimize accordingly

## Performance Considerations

### Memory Management
- **Pagination**: Uses 1000-record limit for large datasets
- **Streaming**: iText7 uses efficient streaming for large documents
- **Cleanup**: Automatic disposal of PDF resources via `using` statements

### File Size Optimization
- **Table Layout**: Optimized column widths for readability
- **Font Selection**: Standard fonts for smaller file sizes
- **Image Exclusion**: Text-only reports for minimal file size

### Execution Time
- **Background Processing**: Long-running reports don't block user requests
- **Asynchronous**: All operations are fully async for better throughput
- **Cron Scheduling**: Minute-based scheduling for timely report generation

## Security Considerations

### File Access
- **Path Validation**: Secure file path handling prevents directory traversal
- **Permission Control**: Appropriate file permissions for generated PDFs
- **Directory Isolation**: Reports stored in designated directories only

### Data Privacy
- **Access Control**: Only authorized systems can generate reports
- **Data Filtering**: Respects user access permissions for activity data
- **Retention Policy**: Automatic cleanup prevents indefinite data storage

### Container Security
- **Volume Mounts**: Limited to necessary directories only
- **User Context**: PDF generation runs with appropriate container permissions
- **Network Isolation**: Background jobs don't require external network access

## Extension Points

### Adding New Report Types

1. **Extend IPdfService**:
```csharp
Task GenerateCustomReportAsync(IEnumerable<CustomData> data, string filePath);
```

2. **Implement in PdfService**:
```csharp
public async Task GenerateCustomReportAsync(IEnumerable<CustomData> data, string filePath)
{
    // Custom PDF generation logic
}
```

3. **Add Console Commands**:
```csharp
public async Task GenerateCustomReportCommandAsync()
{
    await _pdfService.GenerateCustomReportAsync(customData, filePath);
}
```

### Customizing PDF Styling

The current implementation uses helper methods for consistent styling:

```csharp
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
```

These can be extended or overridden for different report styles.

### Adding Report Formats

To support additional formats (Excel, CSV, etc.):

1. Create format-specific services
2. Extend the interface with format parameters
3. Implement format detection logic
4. Add corresponding console commands

## Best Practices

### Development
- **Testing**: Use console commands to test PDF generation during development
- **Logging**: Monitor logs for generation success/failure
- **File Verification**: Always check generated file properties (size, accessibility)

### Production
- **Monitoring**: Set up alerts for background job failures
- **Storage**: Monitor disk space for report directories
- **Performance**: Track generation times and optimize as needed

### Maintenance
- **Regular Cleanup**: Verify automatic cleanup is functioning
- **Version Updates**: Keep iText7 updated for security and features
- **Testing**: Run console test commands after deployments

## Future Enhancements

### Potential Improvements
1. **Multiple Formats**: Support for Excel, CSV export
2. **Report Templates**: Configurable PDF templates
3. **Email Integration**: Automated email delivery of reports
4. **Dashboard Integration**: Web-based report generation interface
5. **Advanced Filtering**: Date range, user, event type filters
6. **Compression**: PDF compression for large reports
7. **Digital Signatures**: PDF signing for authenticity
8. **Batch Processing**: Multiple report generation
9. **Custom Scheduling**: User-configurable report schedules
10. **Report Analytics**: Usage statistics and performance metrics

This implementation provides a solid foundation for PDF generation that can be extended to meet evolving business requirements while maintaining reliability and performance.