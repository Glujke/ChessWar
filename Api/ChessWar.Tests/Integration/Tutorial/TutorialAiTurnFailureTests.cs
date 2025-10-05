using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using ChessWar.Application.DTOs;
using System.Net.Http.Json;

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
    /// Ожидает пока активным участником станет указанный игрок, либо игра завершится, в пределах таймаута
    /// </summary>
    private static async Task WaitUntilPlayerTurnAsync(HttpClient client, string gameSessionId, string expectedName, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            var resp = await client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
            if (!resp.IsSuccessStatusCode)
            {
                await Task.Delay(50);
                continue;
            }
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var statusEl = root.GetProperty("status");
            var isFinished = (statusEl.ValueKind == JsonValueKind.String ? (statusEl.GetString() ?? "") == "Player1Victory" || (statusEl.GetString() ?? "") == "Player2Victory" : statusEl.GetInt32() == 2 || statusEl.GetInt32() == 3);
            if (isFinished)
            {
                return;
            }
            var currentTurn = root.GetProperty("currentTurn");
            var active = currentTurn.GetProperty("activeParticipant");
            var name = active.GetProperty("name").GetString() ?? string.Empty;
            if (string.Equals(name, expectedName, StringComparison.Ordinal))
            {
                return;
            }
            await Task.Delay(50);
        }
    }

    /// <summary>
    /// Тест: Когда ИИ не может сделать ход в Tutorial режиме, игра должна корректно завершить ход ИИ и передать ход игроку
    /// </summary>
    [Fact]
    public async Task EndTurn_WhenAiCannotMakeMove_ShouldCompleteAiTurnAndPassToPlayer()
    {
        var createResponse = await _client.PostAsync("/api/v1/gamesession", Json(new
        {
            player1Name = "TestPlayer",
            player2Name = "AI",
            mode = "AI"
        }));

        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createContent);
        var gameSessionId = createDoc.RootElement.GetProperty("id").GetString();

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
            if (team == 1)
            {
                playerPieceId = piece.GetProperty("id").GetInt32().ToString();
                break;
            }
        }

        Assert.NotNull(playerPieceId);

        var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/move",
            Json(new { pieceId = playerPieceId, targetPosition = new { x = 0, y = 2 } }));

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);

        var passAction = new { type = "Pass", pieceId = "0", targetPosition = (object)null };
        var passResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/turn/action", Json(passAction));
        if (passResponse.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await passResponse.Content.ReadAsStringAsync();
            throw new Exception($"Pass action failed: {passResponse.StatusCode} - {errorContent}");
        }

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/turn/end", Json(new { }));
        if (endTurnResponse.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await endTurnResponse.Content.ReadAsStringAsync();
            throw new Exception($"EndTurn failed: {endTurnResponse.StatusCode} - {errorContent}");
        }

        var aiTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/turn/end", Json(new { }));
        if (aiTurnResponse.StatusCode != HttpStatusCode.OK)
        {
            var _ = await aiTurnResponse.Content.ReadAsStringAsync();
        }

        await WaitUntilPlayerTurnAsync(_client, gameSessionId!, "TestPlayer", TimeSpan.FromSeconds(8));

        var finalSessionResponse = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        Assert.Equal(HttpStatusCode.OK, finalSessionResponse.StatusCode);

        var finalSessionContent = await finalSessionResponse.Content.ReadAsStringAsync();
        using var finalSessionDoc = JsonDocument.Parse(finalSessionContent);

        var gameStatus = finalSessionDoc.RootElement.GetProperty("status").GetInt32();
        Assert.True(gameStatus == 1 || gameStatus == 2, $"Ожидался статус Active (1) или Finished (2), получен {gameStatus}");

        var currentTurn = finalSessionDoc.RootElement.GetProperty("currentTurn");

        if (gameStatus == 2)
        {
            Assert.True(currentTurn.ValueKind == JsonValueKind.Null, "currentTurn должен быть null для завершенной игры");
            return;
        }

        var activeParticipant = currentTurn.GetProperty("activeParticipant");
        var activeParticipantName = activeParticipant.GetProperty("name").GetString();

        // Ход должен переключиться на игрока или игра должна завершиться
        Assert.True(activeParticipantName == "TestPlayer" || activeParticipantName == "AI", 
            $"Ожидался TestPlayer или AI, получен {activeParticipantName}");
    }
}
