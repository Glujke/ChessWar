using Microsoft.AspNetCore.Mvc.Testing;
using System.Text;
using System.Text.Json;
using ChessWar.Application.DTOs;
using ChessWar.Domain.Enums;

namespace ChessWar.Tests.Integration;

public class GameModeIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public GameModeIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task StartTutorial_ShouldReturnOk()
    {
        var request = new CreateTutorialSessionDto
        {
            PlayerId = "player123"
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/tutorial", content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var session = JsonSerializer.Deserialize<TutorialSessionDto>(responseContent, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(session);
        Assert.Equal("Tutorial", session.Mode);
        Assert.NotEmpty(session.SignalRUrl);
    }

    [Fact]
    public async Task StartOnlineGame_ShouldReturnNotImplemented()
    {
        var request = new CreateOnlineSessionDto
        {
            HostPlayerId = "host123",
            MaxPlayers = 2
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/online", content);

        Assert.Equal(System.Net.HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task StartLocalGame_ShouldReturnNotImplemented()
    {
        var request = new CreateLocalSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/local", content);

        Assert.Equal(System.Net.HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task StartAiGame_ShouldReturnOk()
    {
        var request = new CreateAiSessionDto
        {
            PlayerId = "player123",
            Difficulty = AiDifficulty.Hard
        };
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/ai", content);

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPlayerStats_ShouldReturnNotImplemented()
    {
        var playerId = "player123";

        var response = await _client.GetAsync($"/api/v1/game/stats/{playerId}");

        Assert.Equal(System.Net.HttpStatusCode.NotImplemented, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailableModes_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/v1/game/modes");

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("tutorial", content);
        Assert.Contains("online", content);
        Assert.Contains("local", content);
        Assert.Contains("ai", content);
        Assert.Contains("statistics", content);
    }
}
