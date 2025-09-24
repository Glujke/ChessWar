using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ChessWar.Tests.Integration.Tutorial;

public class TutorialBattleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public TutorialBattleIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent JsonBody(object o) => new StringContent(JsonSerializer.Serialize(o), Encoding.UTF8, "application/json");

    [Fact]
    public async Task Start_Should_Create_Battle1_Preset()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial?embed=(game)", JsonBody(new { playerId = "p1" }));
        Assert.Equal(HttpStatusCode.OK, start.StatusCode);
        using var doc = JsonDocument.Parse(await start.Content.ReadAsStringAsync());
        var game = doc.RootElement.GetProperty("_embedded").GetProperty("game");
        var p2 = game.GetProperty("player2").GetProperty("pieces").EnumerateArray().ToList();

        // Король (4,7) и пешки на y=6 (минимум 6 штук)
        Assert.Contains(p2, x => TypeName(x) == "King" && x.GetProperty("position").GetProperty("x").GetInt32() == 4 && x.GetProperty("position").GetProperty("y").GetInt32() == 7);
        var pawns = p2.Where(x => TypeName(x) == "Pawn" && x.GetProperty("position").GetProperty("y").GetInt32() == 6).Count();
        Assert.True(pawns >= 6); // допускаем минимум, т.к. баланс может меняться
    }

    [Fact]
    public async Task Advance_After_Victory_Should_Apply_Battle2_Preset()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial?embed=(game)", JsonBody(new { playerId = "p1" }));
        using var d1 = JsonDocument.Parse(await start.Content.ReadAsStringAsync());
        var gameId = d1.RootElement.GetProperty("gameSessionId").GetString();

        var complete = await _client.PostAsync($"/api/v1/gamesession/{gameId}/complete", new StringContent("\"Player1Victory\"", Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        var trans = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition?embed=(game)", JsonBody(new { action = "advance" }));
        Assert.Equal(HttpStatusCode.OK, trans.StatusCode);
        using var d2 = JsonDocument.Parse(await trans.Content.ReadAsStringAsync());
        var game = d2.RootElement.GetProperty("_embedded").GetProperty("game");
        var p2 = game.GetProperty("player2").GetProperty("pieces").EnumerateArray().ToList();

        // Battle2 должен содержать коня и слона в средней линии
        Assert.Contains(p2, x => TypeName(x) == "Knight");
        Assert.Contains(p2, x => TypeName(x) == "Bishop");
    }

    [Fact]
    public async Task Advance_From_Battle2_Should_Apply_Boss_Preset()
    {
        var start = await _client.PostAsync("/api/v1/game/tutorial?embed=(game)", JsonBody(new { playerId = "p1" }));
        using var d1 = JsonDocument.Parse(await start.Content.ReadAsStringAsync());
        var gameId = d1.RootElement.GetProperty("gameSessionId").GetString();

        // 1-й переход к Battle2
        await _client.PostAsync($"/api/v1/gamesession/{gameId}/complete", new StringContent("\"Player1Victory\"", Encoding.UTF8, "application/json"));
        var t1 = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition?embed=(game)", JsonBody(new { action = "advance" }));
        using var d2 = JsonDocument.Parse(await t1.Content.ReadAsStringAsync());
        var gameId2 = d2.RootElement.GetProperty("gameSessionId").GetString();

        // 2-й переход к Boss
        await _client.PostAsync($"/api/v1/gamesession/{gameId2}/complete", new StringContent("\"Player1Victory\"", Encoding.UTF8, "application/json"));
        var t2 = await _client.PostAsync($"/api/v1/gamesession/{gameId2}/tutorial/transition?embed=(game)", JsonBody(new { action = "advance" }));
        Assert.Equal(HttpStatusCode.OK, t2.StatusCode);
        using var d3 = JsonDocument.Parse(await t2.Content.ReadAsStringAsync());
        var game = d3.RootElement.GetProperty("_embedded").GetProperty("game");
        var p2 = game.GetProperty("player2").GetProperty("pieces").EnumerateArray().ToList();

        // Boss должен содержать ферзя
        Assert.Contains(p2, x => TypeName(x) == "Queen");
    }

    private static string TypeName(JsonElement piece)
    {
        var t = piece.GetProperty("type");
        return t.ValueKind switch
        {
            JsonValueKind.String => t.GetString()!,
            JsonValueKind.Number => MapEnumNumberToName(t.GetInt32()),
            _ => string.Empty
        };
    }

    private static string MapEnumNumberToName(int v) => v switch
    {
        0 => "Pawn",
        1 => "Knight",
        2 => "Bishop",
        3 => "Rook",
        4 => "Queen",
        5 => "King",
        _ => string.Empty
    };
}


