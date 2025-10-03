using ChessWar.Application.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционные тесты для обязательного действия
/// </summary>
public class MandatoryActionIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MandatoryActionIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task EndTurn_ShouldReturnBadRequest_WhenNoActionsPerformed()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameSession = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var sessionId = gameSession!.Id;

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{sessionId}/turn/end", null);

        endTurnResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var errorContent = await endTurnResponse.Content.ReadAsStringAsync();
        errorContent.Should().Contain("хотя бы одного действия");
    }

    [Fact]
    public async Task EndTurn_ShouldSucceed_WhenAtLeastOneActionPerformed()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameSession = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var sessionId = gameSession!.Id;

        var getResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var session = await getResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var pawn = session!.Player1.Pieces.First(p => p.Type == Domain.Enums.PieceType.Pawn);

        var moveRequest = new
        {
            pieceId = pawn.Id.ToString(),
            targetPosition = new { X = pawn.Position.X, Y = pawn.Position.Y + 1 }
        };
        await _client.PostAsJsonAsync($"/api/v1/gamesession/{sessionId}/move", moveRequest);

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{sessionId}/turn/end", null);

        endTurnResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
