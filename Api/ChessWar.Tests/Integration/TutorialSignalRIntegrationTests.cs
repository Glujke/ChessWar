using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using ChessWar.Application.DTOs;

namespace ChessWar.Tests.Integration;

public class TutorialSignalRIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TutorialSignalRIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StartTutorial_ShouldReturnSignalRUrl_AndGroupKey()
    {
        var request = new CreateTutorialSessionDto
        {
            PlayerId = "player123"
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/tutorial", content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("signalRUrl", out var url) && url.ValueKind == JsonValueKind.String);
        Assert.True(root.TryGetProperty("sessionId", out var sessionId) && sessionId.ValueKind == JsonValueKind.String);
    }
}


