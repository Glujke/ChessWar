using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ChessWar.Tests.Integration.Tutorial;

/// <summary>
/// Тесты для обработки ситуаций, когда ИИ не может сделать ход в Tutorial режиме
/// </summary>
public class TutorialAiTurnFailureTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TutorialAiTurnFailureTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private static StringContent Json(object obj) => new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    /// <summary>
    /// Тест: Когда ИИ не может сделать ход в Tutorial режиме, игра должна корректно завершить ход ИИ и передать ход игроку
    /// </summary>
    [Fact]
    public async Task EndTurn_WhenAiCannotMakeMove_ShouldCompleteAiTurnAndPassToPlayer()
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
        
        var pieces = sessionDoc.RootElement.GetProperty("player1").GetProperty("pieces");
        
        string? playerPieceId = null;
        foreach (var piece in pieces.EnumerateArray())
        {
            var team = piece.GetProperty("team").GetInt32();
            if (team == 1) // Elves
            {
                playerPieceId = piece.GetProperty("id").GetInt32().ToString();
                break;
            }
        }
        
        Assert.NotNull(playerPieceId);

        var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/move",
            Json(new { pieceId = playerPieceId, targetPosition = new { x = 0, y = 2 } }));
        
        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/turn/end", Json(new { }));

        Assert.Equal(HttpStatusCode.OK, endTurnResponse.StatusCode);
        
        var finalSessionResponse = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        Assert.Equal(HttpStatusCode.OK, finalSessionResponse.StatusCode);
        
        var finalSessionContent = await finalSessionResponse.Content.ReadAsStringAsync();
        using var finalSessionDoc = JsonDocument.Parse(finalSessionContent);
        
        var gameStatus = finalSessionDoc.RootElement.GetProperty("status").GetInt32();
        Assert.True(gameStatus == 1 || gameStatus == 2, $"Ожидался статус Active (1) или Finished (2), получен {gameStatus}");
        
        var currentTurn = finalSessionDoc.RootElement.GetProperty("currentTurn");
        
        if (gameStatus == 2) // Finished
        {
            Assert.True(currentTurn.ValueKind == JsonValueKind.Null, "currentTurn должен быть null для завершенной игры");
            return; // Завершаем тест, так как игра закончена
        }
        
        Assert.NotNull(currentTurn);
        
        var activeParticipant = currentTurn.GetProperty("activeParticipant");
        var activeParticipantName = activeParticipant.GetProperty("name").GetString();
        
        Assert.Equal("TestPlayer", activeParticipantName);
    }
}
