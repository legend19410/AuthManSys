# Hangfire Background Job System Documentation

## Overview

The AuthManSys application uses Hangfire to automatically generate PDF reports of user activity logs every minute. This document explains the complete architecture and implementation of the background job system.

## Table of Contents

1. [System Architecture](#system-architecture)
2. [Components Overview](#components-overview)
3. [Configuration](#configuration)
4. [Implementation Details](#implementation-details)
5. [File Locations](#file-locations)
6. [Job Scheduling](#job-scheduling)
7. [PDF Generation](#pdf-generation)
8. [Monitoring & Troubleshooting](#monitoring--troubleshooting)
9. [Dependencies](#dependencies)

---

## System Architecture

The background job system consists of several interconnected components:

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Hangfire      │    │  Background     │    │   PDF           │
│   Scheduler     │───▶│  Job Service    │───▶│   Generator     │
│                 │    │                 │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                        │                        │
         │                        │                        │
         ▼                        ▼                        ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│  Memory Storage │    │  Activity Log   │    │  File System    │
│  (Job Queue)    │    │  Repository     │    │  (PDF Files)    │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Components Overview

### 1. **Hangfire Server**
- **Location**: `ServiceCollectionExtensions.cs`
- **Purpose**: Background job processing engine
- **Storage**: In-memory storage (for development)
- **Worker Count**: Dynamically calculated based on CPU cores

### 2. **ActivityLogReportJob**
- **Location**: `src/AuthManSys.Infrastructure/BackgroundJobs/ActivityLogReportJob.cs`
- **Purpose**: Main background job that generates PDF reports
- **Execution**: Every minute via Hangfire scheduler

### 3. **PdfService**
- **Location**: `src/AuthManSys.Infrastructure/Pdf/PdfService.cs`
- **Purpose**: Generates formatted PDF reports using iText7
- **Features**: Table formatting, typography, Jamaica timezone support

### 4. **ActivityLogRepository**
- **Location**: `src/AuthManSys.Infrastructure/Database/Repositories/ActivityLogRepository.cs`
- **Purpose**: Data access layer for user activity logs
- **Methods**: Retrieves logs from last 24 hours

---

## Configuration

### Hangfire Setup

**File**: `src/AuthManSys.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs`

```csharp
services.AddHangfire(config =>
{
    var configuration = config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings();

    if (databaseProvider.ToUpper() == "SQLSERVER")
    {
        configuration.UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        });
    }
    else if (databaseProvider.ToUpper() == "MYSQL")
    {
        // For MySQL, we use in-memory storage as fallback
        configuration.UseMemoryStorage();
    }
});
```

### Dependency Injection Registration

```csharp
// Background Jobs
services.AddScoped<IActivityLogReportJob, ActivityLogReportJob>();

// PDF Service
services.AddScoped<IPdfService, PdfService>();

// Repository
services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
```

### Job Scheduling

**File**: `src/AuthManSys.Api/Program.cs`

```csharp
// Configure recurring jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Schedule the activity report job to run every minute
    recurringJobManager.AddOrUpdate<IActivityLogReportJob>(
        "user-activity-report",
        job => job.GenerateUserActivityReportAsync(),
        Cron.Minutely); // This will run every minute
}
```

---

## Implementation Details

### Background Job Interface

**File**: `src/AuthManSys.Application/Common/Interfaces/IActivityLogReportJob.cs`

```csharp
public interface IActivityLogReportJob
{
    Task GenerateUserActivityReportAsync();
}
```

### Background Job Implementation

**File**: `src/AuthManSys.Infrastructure/BackgroundJobs/ActivityLogReportJob.cs`

#### Key Features:
1. **Automatic File Generation**: Creates timestamped PDF files
2. **Data Retrieval**: Fetches activity logs from last 24 hours
3. **File Cleanup**: Removes reports older than 30 days
4. **Error Handling**: Comprehensive logging and exception management

#### Core Logic:
```csharp
public async Task GenerateUserActivityReportAsync()
{
    var fileName = $"UserActivityReport_{JamaicaTimeHelper.Now:yyyyMMdd_HHmmss}.pdf";
    var reportsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "ActivityLogs");
    var filePath = Path.Combine(reportsDirectory, fileName);

    // Generate PDF report
    await _pdfService.GenerateAllUsersActivityReportAsync(filePath);

    // Cleanup old reports (30+ days)
    CleanupOldReports(reportsDirectory);
}
```

### PDF Generation Process

**File**: `src/AuthManSys.Infrastructure/Pdf/PdfService.cs`

#### Features:
- **Professional Formatting**: Clean table layout with headers
- **Data Columns**: Event Type, User ID, Description, Timestamp, IP Address, Device
- **Typography**: Bold headers, proper spacing, consistent formatting
- **Timezone Handling**: All timestamps shown in Jamaica Time
- **Error Handling**: Comprehensive exception management

#### PDF Structure:
1. **Header**: "User Activity Report" title
2. **Timestamp**: Generation time in Jamaica timezone
3. **Data Table**: 6-column table with activity information
4. **Summary**: Total activity count
5. **Footer**: Empty report message if no data

---

## File Locations

### Development Environment (Docker)

#### Manual Reports (Console Commands):
```
Host: /Users/bryanmowatt/Desktop/Projects/AuthManSys/Reports/ActivityLogs/
Container: /src/Reports/ActivityLogs/
```

#### Automatic Background Job Reports:
```
Host: /Users/bryanmowatt/Desktop/Projects/AuthManSys/src/AuthManSys.Api/Reports/ActivityLogs/
Container: /src/Reports/ActivityLogs/
```

### File Naming Convention

#### Manual Reports:
- Format: `ActivityReport_YYYYMMDD_HHMMSS.pdf`
- Example: `ActivityReport_20251230_141403.pdf`

#### Background Job Reports:
- Format: `UserActivityReport_YYYYMMDD_HHMMSS.pdf`
- Example: `UserActivityReport_20251230_144505.pdf`

### Docker Volume Mount

**File**: `docker-compose.yml`

```yaml
volumes:
  - .:/src       # Mount source for hot reload
  - ./Reports:/src/Reports  # Mount Reports directory for PDF access
```

This ensures PDFs generated inside the Docker container are immediately accessible on the host machine.

---

## Job Scheduling

### Cron Expression
- **Schedule**: `Cron.Minutely` (every minute)
- **Job ID**: `"user-activity-report"`
- **Method**: `job => job.GenerateUserActivityReportAsync()`

### Execution Flow
1. **Trigger**: Hangfire scheduler executes job every minute
2. **Data Fetch**: Repository retrieves activity logs from last 24 hours
3. **PDF Creation**: PdfService generates formatted report
4. **File Save**: PDF saved with timestamped filename
5. **Cleanup**: Old reports (30+ days) are automatically deleted
6. **Logging**: Success/failure logged for monitoring

### Storage Behavior
- **Memory Storage**: Jobs are stored in-memory (development setup)
- **Persistence**: Jobs are re-registered on application restart
- **Queue**: Default queue processes jobs sequentially

---

## PDF Generation

### Data Source
- **Repository**: `IActivityLogRepository`
- **Method**: `GetAllActivitiesAsync(fromDate, toDate, pageNumber, pageSize)`
- **Time Range**: Last 24 hours from current time
- **Page Size**: 1000 records maximum per report

### PDF Library
- **Library**: iText7 v9.4.0
- **Dependencies**:
  - `itext7` - Core PDF functionality
  - `itext7.bouncy-castle-adapter` - Security and cryptography support

### Report Content
1. **Title**: "User Activity Report"
2. **Generation Time**: Current timestamp in Jamaica timezone
3. **Activity Table**: 6-column table with the following fields:
   - **Event Type**: Type of activity (Login, Registration, etc.)
   - **User ID**: Unique identifier of the user (or "N/A" for anonymous)
   - **Description**: Human-readable activity description
   - **Timestamp**: When the activity occurred
   - **IP Address**: Source IP address of the activity
   - **Device**: Device/browser information
4. **Summary**: Total count of activities included in report

### Error Handling
- **File System Errors**: Directory creation, file write permissions
- **Data Access Errors**: Database connectivity, query failures
- **PDF Generation Errors**: Library exceptions, formatting issues
- **Cleanup Errors**: File deletion permissions, disk space

---

## Monitoring & Troubleshooting

### Log Messages

#### Successful Execution:
```
info: AuthManSys.Infrastructure.BackgroundJobs.ActivityLogReportJob[0]
      Starting background job: Generate User Activity Report

info: AuthManSys.Infrastructure.BackgroundJobs.ActivityLogReportJob[0]
      Successfully completed background job: User Activity Report generated at /src/Reports/ActivityLogs/UserActivityReport_20251230_144505.pdf
```

#### PDF Generation:
```
info: AuthManSys.Infrastructure.Pdf.PdfService[0]
      Generating all users activity PDF report at /src/Reports/ActivityLogs/UserActivityReport_20251230_144505.pdf

info: AuthManSys.Infrastructure.Pdf.PdfService[0]
      Successfully generated PDF report with 75 entries at /src/Reports/ActivityLogs/UserActivityReport_20251230_144505.pdf
```

### Hangfire Dashboard

**Access**: `http://localhost:8081/hangfire` (Development only)
**Features**:
- Job queue monitoring
- Execution history
- Failed job analysis
- Performance metrics

**Security**: Dashboard is only accessible in Development environment for security reasons.

### Common Issues & Solutions

#### 1. **No PDFs Generated**
- **Check**: Hangfire server status in logs
- **Look for**: `Listening queues: 'default'` message
- **Solution**: Verify Hangfire configuration and storage setup

#### 2. **PDF Generation Failures**
- **Check**: BouncyCastle dependency installation
- **Error**: `UseMemoryStorage` not found
- **Solution**: Ensure `Hangfire.MemoryStorage` package is installed

#### 3. **File Access Issues**
- **Check**: Docker volume mounts in `docker-compose.yml`
- **Verify**: Directory permissions and disk space
- **Solution**: Ensure proper volume mapping and container permissions

#### 4. **Database Connection Errors**
- **Check**: MySQL connection string and server health
- **Verify**: Entity Framework DbContext registration
- **Solution**: Check database connectivity and repository configuration

### Verification Commands

```bash
# Check if background job is running
docker compose logs authman-api | grep -i "background\|hangfire"

# List generated PDF files
ls -la ./src/AuthManSys.Api/Reports/ActivityLogs/

# Monitor real-time logs
docker compose logs authman-api --follow

# Check Hangfire dashboard (development)
curl http://localhost:8081/hangfire
```

---

## Dependencies

### NuGet Packages

**File**: `src/AuthManSys.Infrastructure/AuthManSys.Infrastructure.csproj`

```xml
<!-- Hangfire Background Jobs -->
<PackageReference Include="Hangfire.AspNetCore" Version="1.8.22" />
<PackageReference Include="Hangfire.Core" Version="1.8.22" />
<PackageReference Include="Hangfire.MemoryStorage" Version="1.8.0" />
<PackageReference Include="Hangfire.SqlServer" Version="1.8.22" />

<!-- PDF Generation -->
<PackageReference Include="itext7" Version="9.4.0" />
<PackageReference Include="itext7.bouncy-castle-adapter" Version="9.4.0" />
```

### System Requirements
- **.NET 9.0**: Target framework
- **Entity Framework Core 9.0**: Data access
- **MySQL/SQL Server**: Database backend
- **Docker**: Containerized deployment
- **Sufficient Disk Space**: For PDF file storage

### Environment Variables
- `ASPNETCORE_ENVIRONMENT=Development` - Enables Hangfire dashboard
- `DatabaseProvider=MySQL` - Configures storage provider
- `DOTNET_RUNNING_IN_CONTAINER=true` - Container detection

---

## Summary

The Hangfire background job system provides a robust, automated solution for generating user activity reports. Key benefits include:

- **Automated Execution**: No manual intervention required
- **Reliable Scheduling**: Consistent minute-by-minute execution
- **Professional Output**: Well-formatted PDF reports
- **Comprehensive Logging**: Full audit trail of job execution
- **Error Recovery**: Automatic retry mechanisms via Hangfire
- **File Management**: Automatic cleanup of old reports
- **Container Support**: Full Docker compatibility with volume mounts

The system is production-ready and can be easily modified to support different schedules, report formats, or data sources by adjusting the configuration and implementation components described in this document.