using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using ChessWar.Application.DTOs;

namespace ChessWar.Tests.Integration;

public class TutorialContractIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TutorialContractIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StartTutorial_Response_ShouldContain_RequiredFields_ForClient()
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

        Assert.True(root.TryGetProperty("sessionId", out var sessionId) && sessionId.ValueKind == JsonValueKind.String);
        Assert.True(root.TryGetProperty("mode", out var mode) && mode.GetString() == "Tutorial");
        Assert.True(root.TryGetProperty("stage", out var stage) && stage.ValueKind == JsonValueKind.String);
        Assert.True(root.TryGetProperty("progress", out var progress) && progress.ValueKind == JsonValueKind.Number);
        Assert.True(root.TryGetProperty("signalRUrl", out var signalRUrl) && signalRUrl.ValueKind == JsonValueKind.String);

        Assert.True(root.TryGetProperty("scenario", out var scenario) && scenario.ValueKind == JsonValueKind.Object);
        Assert.True(scenario.TryGetProperty("type", out var scenarioType) && scenarioType.ValueKind == JsonValueKind.String);
        Assert.True(scenario.TryGetProperty("difficulty", out var difficulty) && difficulty.ValueKind == JsonValueKind.String);

        Assert.True(root.TryGetProperty("board", out var board) && board.ValueKind == JsonValueKind.Object);
        Assert.True(root.TryGetProperty("pieces", out var pieces) && pieces.ValueKind is JsonValueKind.Array);

        Assert.True(board.TryGetProperty("width", out var width) && width.ValueKind == JsonValueKind.Number);
        Assert.True(board.TryGetProperty("height", out var height) && height.ValueKind == JsonValueKind.Number);
    }
}


