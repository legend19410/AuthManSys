using AuthManSys.Application.Common.Interfaces;
using AuthManSys.Domain.Entities;
using AuthManSys.Domain.Enums;
using AuthManSys.Application.Common.Helpers;

namespace AuthManSys.Console.Commands;

public class PdfCommands : IPdfCommands
{
    private readonly IPdfService _pdfService;
    private readonly IActivityLogRepository _activityLogRepository;

    public PdfCommands(IPdfService pdfService, IActivityLogRepository activityLogRepository)
    {
        _pdfService = pdfService;
        _activityLogRepository = activityLogRepository;
    }

    public async Task TestPdfGenerationAsync()
    {
        System.Console.WriteLine("📄 Testing PDF Generation");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        try
        {
            // Create some test activity logs first
            await CreateTestActivityLogsAsync();

            // Generate PDF report
            var fileName = $"TestReport_{JamaicaTimeHelper.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "Test", fileName);

            System.Console.WriteLine($"Generating PDF report: {fileName}");

            await _pdfService.GenerateAllUsersActivityReportAsync(filePath);

            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                System.Console.WriteLine("✅ PDF generated successfully!");
                System.Console.WriteLine($"📁 File: {filePath}");
                System.Console.WriteLine($"📏 Size: {fileInfo.Length:N0} bytes");
                System.Console.WriteLine($"🕒 Created: {fileInfo.CreationTime}");
            }
            else
            {
                System.Console.WriteLine("❌ PDF file was not created.");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error generating PDF: {ex.Message}");
            System.Console.WriteLine($"📋 Stack trace: {ex.StackTrace}");
        }
    }

    public async Task GenerateTestReportAsync()
    {
        System.Console.WriteLine("📊 Generating Test Activity Report");
        System.Console.WriteLine("─────────────────────────────────────────────────────────────────");

        try
        {
            // Create test data with more variety
            await CreateDiverseTestDataAsync();

            // Generate the report
            var fileName = $"ActivityReport_{JamaicaTimeHelper.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "ActivityLogs", fileName);

            System.Console.WriteLine($"📝 Creating comprehensive activity report: {fileName}");

            await _pdfService.GenerateAllUsersActivityReportAsync(filePath);

            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                System.Console.WriteLine("✅ Activity report generated successfully!");
                System.Console.WriteLine($"📁 Location: {filePath}");
                System.Console.WriteLine($"📏 File Size: {fileInfo.Length:N0} bytes");
                System.Console.WriteLine($"🕒 Generated: {fileInfo.CreationTime}");

                // Show activity log count
                var recentLogs = await _activityLogRepository.GetAllActivitiesAsync(
                    fromDate: JamaicaTimeHelper.Now.AddDays(-1),
                    pageSize: 1000);

                System.Console.WriteLine($"📊 Activities included: {recentLogs.Count()} from last 24 hours");
            }
            else
            {
                System.Console.WriteLine("❌ Report file was not created.");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error generating report: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"🔍 Inner exception: {ex.InnerException.Message}");
            }
        }
    }

    private async Task CreateTestActivityLogsAsync()
    {
        System.Console.WriteLine("🔄 Creating test activity logs...");

        var testActivities = new[]
        {
            (ActivityEventType.LoginSuccess, "User logged in successfully", "127.0.0.1", "Test Browser"),
            (ActivityEventType.UserRegistered, "New user registered", "127.0.0.1", "Chrome"),
            (ActivityEventType.PasswordChanged, "Password changed", "192.168.1.1", "Firefox"),
            (ActivityEventType.TwoFactorEnabled, "Two-factor authentication enabled", "10.0.0.1", "Safari"),
            (ActivityEventType.EmailConfirmed, "Email address verified", "172.16.0.1", "Edge")
        };

        foreach (var (eventType, description, ip, device) in testActivities)
        {
            await _activityLogRepository.LogActivityAsync(
                userId: 1,
                eventType: eventType,
                description: description,
                ipAddress: ip,
                device: device,
                platform: "TestPlatform",
                location: "Test Location",
                metadata: new { TestData = true, Source = "Console" });
        }

        System.Console.WriteLine($"✅ Created {testActivities.Length} test activity logs");
    }

    private async Task CreateDiverseTestDataAsync()
    {
        System.Console.WriteLine("🎯 Creating diverse test data...");

        var testUsers = new int?[] { 1, 2, 3, null }; // Including some anonymous activities
        var ipAddresses = new[] { "192.168.1.100", "10.0.0.50", "172.16.0.200", "203.0.113.1" };
        var devices = new[] { "Chrome/Win10", "Safari/macOS", "Firefox/Linux", "Edge/Win11", "Mobile/iOS" };
        var platforms = new[] { "Web", "Mobile", "Desktop", "API" };
        var locations = new[] { "New York, USA", "London, UK", "Tokyo, Japan", "Sydney, Australia" };

        var eventTypes = Enum.GetValues<ActivityEventType>();
        var random = new Random();

        // Create 20 varied activity logs
        for (int i = 0; i < 20; i++)
        {
            await _activityLogRepository.LogActivityAsync(
                userId: testUsers[random.Next(testUsers.Length)],
                eventType: eventTypes[random.Next(eventTypes.Length)],
                description: $"Test activity #{i + 1} - {eventTypes[random.Next(eventTypes.Length)]} operation performed",
                ipAddress: ipAddresses[random.Next(ipAddresses.Length)],
                device: devices[random.Next(devices.Length)],
                platform: platforms[random.Next(platforms.Length)],
                location: locations[random.Next(locations.Length)],
                metadata: new {
                    TestIndex = i + 1,
                    Source = "Console Test",
                    Timestamp = JamaicaTimeHelper.Now,
                    RandomValue = random.Next(1000, 9999)
                });
        }

        System.Console.WriteLine("✅ Created 20 diverse test activity logs");
    }
}