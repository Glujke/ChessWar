using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class AiBehaviorIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public AiBehaviorIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task AiTurn_ShouldBeAvailable_In_AI_Mode_AndNotIn_LocalCoop()
    {
        var createAi = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "AI", Mode = "AI" };
        var respAi = await _client.PostAsJsonAsync("/api/v1/gamesession", createAi);
        var sessionAi = await respAi.Content.ReadFromJsonAsync<GameSessionDto>();

        // Проверяем, что сессия создана правильно
        respAi.StatusCode.Should().Be(HttpStatusCode.OK);
        sessionAi.Should().NotBeNull();
        sessionAi!.Mode.Should().Be("AI");

        // Сначала выполняем действие игрока, чтобы переключиться на ИИ
        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{sessionAi.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var piece = state!.Player1.Pieces.First();
        
        var moveAction = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = piece.Id.ToString(),
            TargetPosition = new PositionDto { X = piece.Position.X, Y = piece.Position.Y + 1 }
        };
        
        var moveResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{sessionAi.Id}/turn/action", moveAction);
        moveResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var endTurn = await _client.PostAsync($"/api/v1/gamesession/{sessionAi.Id}/turn/end", null);
        endTurn.StatusCode.Should().Be(HttpStatusCode.OK);

        var aiTurn = await _client.PostAsync($"/api/v1/gamesession/{sessionAi.Id}/turn/ai", null);
        aiTurn.StatusCode.Should().Be(HttpStatusCode.OK);

        var createLc = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "LocalCoop" };
        var respLc = await _client.PostAsJsonAsync("/api/v1/gamesession", createLc);
        var sessionLc = await respLc.Content.ReadFromJsonAsync<GameSessionDto>();

        var lcTurn = await _client.PostAsync($"/api/v1/gamesession/{sessionLc!.Id}/turn/ai", null);
        lcTurn.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}


