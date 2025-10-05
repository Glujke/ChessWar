using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChessWar.Tests.Integration.Tutorial;

/// <summary>
/// Тесты для проверки, что ИИ не может двигать фигуры игрока
/// </summary>
public class AiPlayerPieceProtectionTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AiPlayerPieceProtectionTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent Json(object obj) => new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    /// <summary>
    /// Тест: ИИ НЕ должен двигать фигуры игрока в Tutorial режиме
    /// </summary>
    [Fact]
    public async Task AiTurn_ShouldNotMovePlayerPieces_InTutorialMode()
    {
        var createResponse = await _client.PostAsync("/api/v1/game/tutorial?embed=(game)", Json(new { playerId = "TestPlayer" }));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createContent);
        var gameSessionId = createDoc.RootElement.GetProperty("gameSessionId").GetString();

        Assert.NotNull(gameSessionId);

        var sessionResponse = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);

        var sessionContent = await sessionResponse.Content.ReadAsStringAsync();
        using var sessionDoc = JsonDocument.Parse(sessionContent);

        var playerPieces = new List<(int Id, int X, int Y)>();
        var pieces = sessionDoc.RootElement.GetProperty("player1").GetProperty("pieces");
        foreach (var piece in pieces.EnumerateArray())
        {
            var team = piece.GetProperty("team").GetInt32();
            if (team == 1)
            {
                var id = piece.GetProperty("id").GetInt32();
                var position = piece.GetProperty("position");
                var x = position.GetProperty("x").GetInt32();
                var y = position.GetProperty("y").GetInt32();
                playerPieces.Add((id, x, y));
            }
        }

        Assert.True(playerPieces.Count > 0, "Player should have pieces");

        var playerPieceId = playerPieces.First().Id.ToString();
        var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/move",
            Json(new { pieceId = playerPieceId, targetPosition = new { x = 0, y = 2 } }));

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);

        var beforeAiResponse = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        Assert.Equal(HttpStatusCode.OK, beforeAiResponse.StatusCode);

        var beforeAiContent = await beforeAiResponse.Content.ReadAsStringAsync();
        using var beforeAiDoc = JsonDocument.Parse(beforeAiContent);
        var beforeAiPlayerPieces = GetPlayerPieces(beforeAiDoc.RootElement);

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/turn/end", Json(new { }));

        Assert.Equal(HttpStatusCode.OK, endTurnResponse.StatusCode);

        var afterAiResponse = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        Assert.Equal(HttpStatusCode.OK, afterAiResponse.StatusCode);

        var afterAiContent = await afterAiResponse.Content.ReadAsStringAsync();
        using var afterAiDoc = JsonDocument.Parse(afterAiContent);
        var afterAiPlayerPieces = GetPlayerPieces(afterAiDoc.RootElement);

        foreach (var beforePiece in beforeAiPlayerPieces)
        {
            var afterPiece = afterAiPlayerPieces.FirstOrDefault(p => p.Id == beforePiece.Id);
            Assert.True(afterPiece.Id != 0, $"Player piece {beforePiece.Id} not found after AI turn");
            Assert.Equal(beforePiece.Position.X, afterPiece.Position.X);
            Assert.Equal(beforePiece.Position.Y, afterPiece.Position.Y);
        }
    }

    private static List<(int Id, (int X, int Y) Position)> GetPlayerPieces(JsonElement rootElement)
    {
        var playerPieces = new List<(int Id, (int X, int Y) Position)>();
        var pieces = rootElement.GetProperty("player1").GetProperty("pieces");
        foreach (var piece in pieces.EnumerateArray())
        {
            var team = piece.GetProperty("team").GetInt32();
            if (team == 1)
            {
                var id = piece.GetProperty("id").GetInt32();
                var position = piece.GetProperty("position");
                var x = position.GetProperty("x").GetInt32();
                var y = position.GetProperty("y").GetInt32();
                playerPieces.Add((id, (x, y)));
            }
        }
        return playerPieces;
    }
}
