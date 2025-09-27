using ChessWar.Application.DTOs;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционные тесты для системы маны игрока
/// </summary>
public class PlayerManaIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public PlayerManaIntegrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateGameSession_ShouldInitializePlayerMana()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var response = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var gameSession = await response.Content.ReadFromJsonAsync<GameSessionDto>();
        gameSession.Should().NotBeNull();
        
        gameSession!.Player1.MP.Should().Be(10); // Начальная мана
        gameSession.Player1.MaxMP.Should().Be(50); // Максимальная мана
        gameSession.Player2.MP.Should().Be(10);
        gameSession.Player2.MaxMP.Should().Be(50);
    }

    [Fact]
    public async Task MovePiece_ShouldConsumePlayerMana()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
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

        var moveResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{sessionId}/move", moveRequest);

        moveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var afterMoveResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var afterSession = await afterMoveResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        
        afterSession!.Player1.MP.Should().Be(9); // 10 - 1
        afterSession.Player1.MaxMP.Should().Be(50);
    }

    [Fact]
    public async Task MovePiece_ShouldReturnBadRequest_WhenNotEnoughMana()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
        var gameSession = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var sessionId = gameSession!.Id;

        var getResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var session = await getResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var king = session!.Player1.Pieces.First(p => p.Type == Domain.Enums.PieceType.King);

        for (int i = 0; i < 10; i++) // Делаем 10 ходов пешками (10 мана)
        {
            var pawn = session.Player1.Pieces.FirstOrDefault(p => p.Type == Domain.Enums.PieceType.Pawn && p.Position.Y == 1);
            if (pawn != null)
            {
                var moveRequest = new
                {
                    pieceId = pawn.Id.ToString(),
                    targetPosition = new { X = pawn.Position.X, Y = pawn.Position.Y + 1 }
                };
                await _client.PostAsJsonAsync($"/api/v1/gamesession/{sessionId}/move", moveRequest);
            }
        }

        var kingMoveRequest = new
        {
            pieceId = king.Id.ToString(),
            targetPosition = new { X = king.Position.X, Y = king.Position.Y + 1 }
        };

        var moveResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{sessionId}/move", kingMoveRequest);

        moveResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EndTurn_ShouldRestorePlayerMana()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
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
        
        var afterEndTurnResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var afterSession = await afterEndTurnResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        
        afterSession!.Player1.MP.Should().Be(19); // осталось 9; реген +10 уходит следующему активному игроку (AI), который может сразу потратить ману, но теперь игрок тоже получает реген
        afterSession.Player1.MaxMP.Should().Be(50);
    }

    [Fact]
    public async Task EndTurn_ShouldNotExceedMaxMana()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
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
        
        var afterEndTurnResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var afterSession = await afterEndTurnResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        
        afterSession!.Player1.MP.Should().BeLessOrEqualTo(50); // Не больше максимума
        afterSession.Player1.MaxMP.Should().Be(50);
    }

    [Fact]
    public async Task EndTurn_ShouldRestorePlayerMana_AfterAITurn()
    {
        var createRequest = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "AI",
            Mode = "AI"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createRequest);
        var gameSession = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var sessionId = gameSession!.Id;

        var initialResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var initialSession = await initialResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        var initialPlayerMana = initialSession!.Player1.MP;

        var pawn = initialSession.Player1.Pieces.First(p => p.Type == Domain.Enums.PieceType.Pawn);
        var moveRequest = new
        {
            pieceId = pawn.Id.ToString(),
            targetPosition = new { X = pawn.Position.X, Y = pawn.Position.Y + 1 }
        };
        await _client.PostAsJsonAsync($"/api/v1/gamesession/{sessionId}/move", moveRequest);

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{sessionId}/turn/end", null);

        endTurnResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var finalResponse = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        var finalSession = await finalResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        
        finalSession!.CurrentTurn.ActiveParticipant.Name.Should().Be("Player1");
        
        finalSession.Player1.MP.Should().BeGreaterThan(initialPlayerMana, 
            "мана игрока должна быть восстановлена после хода ИИ");
        
        finalSession.Player1.MP.Should().BeLessOrEqualTo(50);
    }
}
