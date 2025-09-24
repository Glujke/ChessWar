using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChessWar.Tests.Integration.Tutorial;

public class TutorialStartTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TutorialStartTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Tutorial_With_Embed_Should_Return_200_With_Location_And_Embedded_Game()
    {
        var payload = JsonSerializer.Serialize(new { playerId = "player-123" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/tutorial?embed=(game)", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Location", out var locVals) && locVals.Any(), "Location header is required");
        var location = locVals.First();
        Assert.StartsWith("/api/v1/game/tutorial/", location);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("gameSessionId", out var gameId) && gameId.ValueKind == JsonValueKind.String);
        if (root.TryGetProperty("_embedded", out var embedded))
        {
            Assert.True(embedded.TryGetProperty("game", out var game) && game.ValueKind == JsonValueKind.Object);
        }
        else
        {
            // embed requested => _embedded must be present
            Assert.Fail("_embedded expected when embed=(game)");
        }
    }

    [Fact]
    public async Task Post_Tutorial_Without_Embed_Should_Return_200_Without_Embedded()
    {
        var payload = JsonSerializer.Serialize(new { playerId = "player-123" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/tutorial", content);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Location", out var _), "Location header is required");

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.True(root.TryGetProperty("gameSessionId", out _));
        Assert.False(root.TryGetProperty("_embedded", out _), "_embedded must be absent when embed is not requested");
    }
}


