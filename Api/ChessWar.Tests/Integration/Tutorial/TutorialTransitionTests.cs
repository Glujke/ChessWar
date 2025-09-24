using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChessWar.Tests.Integration.Tutorial;

public class TutorialTransitionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TutorialTransitionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent Json(object obj) => new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    [Fact]
    public async Task Transition_Advance_Should_Return_409_When_Game_Not_Completed()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial", Json(new { playerId = "p1" }));
        var startJson = await start.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(startJson);
        var gameId = doc.RootElement.GetProperty("gameSessionId").GetString();

        var resp = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition", Json(new { action = "advance" }));
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
        var body = await resp.Content.ReadAsStringAsync();
        using var p = JsonDocument.Parse(body);
        Assert.Equal("StageNotCompleted", p.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Transition_Replay_Should_Return_200_And_New_GameSession()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial", Json(new { playerId = "p1" }));
        var startJson = await start.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(startJson);
        var gameId = doc.RootElement.GetProperty("gameSessionId").GetString();

        var resp = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition", Json(new { action = "replay" }));
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        using var j = JsonDocument.Parse(body);
        var newGameId = j.RootElement.GetProperty("gameSessionId").GetString();
        Assert.NotNull(newGameId);
        Assert.NotEqual(gameId, newGameId);
    }

    [Fact]
    public async Task Transition_Advance_Should_Return_409_When_Game_Completed_With_Defeat()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial", Json(new { playerId = "p1" }));
        var startJson = await start.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(startJson);
        var gameId = doc.RootElement.GetProperty("gameSessionId").GetString();

        // Завершить игру поражением игрока (победа второго игрока)
        var defeatBody = new StringContent("\"Player2Victory\"", Encoding.UTF8, "application/json");
        var complete = await _client.PostAsync($"/api/v1/gamesession/{gameId}/complete", defeatBody);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        var resp = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition", Json(new { action = "advance" }));
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
        Assert.Equal("application/problem+json", resp.Content.Headers.ContentType?.MediaType);
        var body = await resp.Content.ReadAsStringAsync();
        using var p = JsonDocument.Parse(body);
        Assert.Equal("StageNotCompleted", p.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Transition_Advance_Should_Return_200_With_Embed_When_Game_Completed_With_Victory()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial", Json(new { playerId = "p1" }));
        var startJson = await start.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(startJson);
        var gameId = doc.RootElement.GetProperty("gameSessionId").GetString();

        // Завершить игру победой игрока (победа первого игрока)
        var victoryBody = new StringContent("\"Player1Victory\"", Encoding.UTF8, "application/json");
        var complete = await _client.PostAsync($"/api/v1/gamesession/{gameId}/complete", victoryBody);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        var resp = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition?embed=(game)", Json(new { action = "advance" }));
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var json = await resp.Content.ReadAsStringAsync();
        using var j = JsonDocument.Parse(json);
        var newGameId = j.RootElement.GetProperty("gameSessionId").GetString();
        Assert.False(string.IsNullOrEmpty(newGameId));
        Assert.NotEqual(gameId, newGameId);

        Assert.True(j.RootElement.TryGetProperty("_embedded", out var embedded));
        Assert.True(embedded.TryGetProperty("game", out var game) && game.ValueKind == JsonValueKind.Object);
    }
}


