using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChessWar.Tests.Integration.Middleware;

public class ProblemDetailsIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProblemDetailsIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_For_Game_Exception()
    {
        // Arrange
        var request = new
        {
            pieceId = Guid.NewGuid(),
            requiredMp = 5,
            availableMp = 3
        };

        // Act
        var response = await _client.PostAsync("/api/v1/test/insufficient-mp", 
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content);
        
        problemDetails!.Status.Should().Be(400);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Detail.Should().Contain("Insufficient MP");
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be("InsufficientMp");
    }

    [Fact]
    public async Task Should_Return_ProblemDetails_For_Stage_Not_Completed()
    {
        // Arrange
        var request = new
        {
            stageName = "Battle1",
            requiredCondition = "Defeat all enemies"
        };

        // Act
        var response = await _client.PostAsync("/api/v1/test/stage-not-completed", 
            new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");

        var content = await response.Content.ReadAsStringAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content);
        
        problemDetails!.Status.Should().Be(409);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.9");
        problemDetails.Title.Should().Be("Conflict");
        problemDetails.Detail.Should().Contain("Stage 'Battle1' is not completed");
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be("StageNotCompleted");
    }
}
