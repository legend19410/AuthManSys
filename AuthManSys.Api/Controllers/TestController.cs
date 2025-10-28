using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthManSys.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<TestData>> GetTestData()
    {
        var username = User.FindFirst("username")?.Value ?? "Unknown";
        
        var testData = new[]
        {
            new TestData(1, $"Sample Item 1 for {username}", "This is a test item", DateTime.UtcNow),
            new TestData(2, $"Sample Item 2 for {username}", "Another test item", DateTime.UtcNow.AddHours(-1)),
            new TestData(3, $"Sample Item 3 for {username}", "Third test item", DateTime.UtcNow.AddHours(-2))
        };

        return Ok(testData);
    }

    [HttpGet("{id}")]
    public ActionResult<TestData> GetTestDataById(int id)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than 0");

        var testItem = new TestData(id, $"Sample Item {id}", $"Test item with ID {id}", DateTime.UtcNow);
        return Ok(testItem);
    }

    [HttpPost]
    public ActionResult<TestData> CreateTestData([FromBody] CreateTestDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required");

        var testItem = new TestData(
            Random.Shared.Next(1000, 9999),
            request.Name,
            request.Description ?? "No description provided",
            DateTime.UtcNow
        );

        return CreatedAtAction(nameof(GetTestDataById), new { id = testItem.Id }, testItem);
    }
}

public record TestData(int Id, string Name, string Description, DateTime CreatedAt);

public record CreateTestDataRequest(string Name, string? Description);