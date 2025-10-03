using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционный тест для проверки атак на мёртвые фигуры
/// TDD подход: сначала пишем КРАСНЫЙ тест, потом делаем ЗЕЛЕНЫМ
/// </summary>
public class DeadPieceAttackIntegrationTest : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly IGameSessionManagementService _sessionService;
    private readonly IActionExecutionService _actionExecutionService;

    public DeadPieceAttackIntegrationTest(TestWebApplicationFactory factory) : base(factory)
    {
        _sessionService = _scope.ServiceProvider.GetRequiredService<IGameSessionManagementService>();
        _actionExecutionService = _scope.ServiceProvider.GetRequiredService<IActionExecutionService>();
    }

    [Fact]
    public async Task AttackDeadPiece_ShouldReturn400Error()
    {
        var payload = JsonSerializer.Serialize(new { playerId = "test-player-123" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/tutorial", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var gameSessionId = doc.RootElement.GetProperty("gameSessionId").GetString();
        Assert.NotNull(gameSessionId);

        var sessionId = Guid.Parse(gameSessionId);
        var session = await _sessionService.GetSessionAsync(sessionId);
        Assert.NotNull(session);

        var playerPawn = session.Player1.Pieces.FirstOrDefault(p =>
            p.Type == PieceType.Pawn && p.Position.X == 0 && p.Position.Y == 1);

        Assert.NotNull(playerPawn);

        var enemyPiece = session.Player2.Pieces.FirstOrDefault(p =>
            p.Position.X == 0 && p.Position.Y == 6);

        Assert.NotNull(enemyPiece);
        Assert.True(enemyPiece.IsAlive); // Изначально живая

        var moveAction1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 2 }
        };

        var moveResult1 = await _actionExecutionService.ExecuteActionAsync(session, moveAction1);
        Assert.True(moveResult1); // Движение должно быть успешным

        var moveAction2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 3 }
        };

        var moveResult2 = await _actionExecutionService.ExecuteActionAsync(session, moveAction2);
        Assert.True(moveResult2);

        var moveAction3 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 4 }
        };

        var moveResult3 = await _actionExecutionService.ExecuteActionAsync(session, moveAction3);
        Assert.True(moveResult3);

        var moveAction4 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 5 }
        };

        var moveResult4 = await _actionExecutionService.ExecuteActionAsync(session, moveAction4);
        Assert.True(moveResult4);

        var attackAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 6 }
        };

        var attackResult = await _actionExecutionService.ExecuteActionAsync(session, attackAction);
        Assert.True(attackResult); // Первая атака должна быть успешной

        for (int i = 0; i < 4; i++) // Уже сделали 1 атаку выше
        {
            var attackAgain = new ExecuteActionDto
            {
                Type = "Attack",
                PieceId = playerPawn.Id.ToString(),
                TargetPosition = new PositionDto { X = 0, Y = 6 }
            };

            var attackAgainResult = await _actionExecutionService.ExecuteActionAsync(session, attackAgain);
            Assert.True(attackAgainResult);
        }

        var finalEnemyPiece = session.Player2.Pieces.FirstOrDefault(p => p.Id == enemyPiece.Id);
        Assert.NotNull(finalEnemyPiece);
        Assert.False(finalEnemyPiece.IsAlive); // Теперь мёртвая!

        var attackDeadAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 6 }
        };

        var attackDeadResult = await _actionExecutionService.ExecuteActionAsync(session, attackDeadAction);

        Assert.False(attackDeadResult); // Атака мёртвой фигуры должна возвращать false
    }

    [Fact]
    public async Task MoveToDeadPiecePosition_ShouldReturn200Success()
    {
        var payload = JsonSerializer.Serialize(new { playerId = "test-player-456" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/game/tutorial", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var gameSessionId = doc.RootElement.GetProperty("gameSessionId").GetString();
        Assert.NotNull(gameSessionId);

        var sessionId = Guid.Parse(gameSessionId);
        var session = await _sessionService.GetSessionAsync(sessionId);
        Assert.NotNull(session);

        var playerPawn = session.Player1.Pieces.FirstOrDefault(p =>
            p.Type == PieceType.Pawn && p.Position.X == 0 && p.Position.Y == 1);

        var enemyPiece = session.Player2.Pieces.FirstOrDefault(p =>
            p.Position.X == 0 && p.Position.Y == 6);

        Assert.NotNull(playerPawn);
        Assert.NotNull(enemyPiece);

        var moveAction1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 2 }
        };

        var moveResult1 = await _actionExecutionService.ExecuteActionAsync(session, moveAction1);
        Assert.True(moveResult1);

        var moveAction2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 3 }
        };

        var moveResult2 = await _actionExecutionService.ExecuteActionAsync(session, moveAction2);
        Assert.True(moveResult2);

        var moveAction3 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 4 }
        };

        var moveResult3 = await _actionExecutionService.ExecuteActionAsync(session, moveAction3);
        Assert.True(moveResult3);

        var moveAction4 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 5 }
        };

        var moveResult4 = await _actionExecutionService.ExecuteActionAsync(session, moveAction4);
        Assert.True(moveResult4);

        for (int i = 0; i < 5; i++)
        {
            var attackAction = new ExecuteActionDto
            {
                Type = "Attack",
                PieceId = playerPawn.Id.ToString(),
                TargetPosition = new PositionDto { X = 0, Y = 6 }
            };

            var attackResult = await _actionExecutionService.ExecuteActionAsync(session, attackAction);
            Assert.True(attackResult);
        }

        var deadEnemyPiece = session.Player2.Pieces.FirstOrDefault(p => p.Id == enemyPiece.Id);
        Assert.NotNull(deadEnemyPiece);
        Assert.False(deadEnemyPiece.IsAlive);

        var updatedPlayerPawn = session.Player1.Pieces.FirstOrDefault(p => p.Id == playerPawn.Id);
        Assert.NotNull(updatedPlayerPawn);
        Assert.Equal(0, updatedPlayerPawn.Position.X);
        Assert.Equal(6, updatedPlayerPawn.Position.Y);

        Assert.True(true); // Механика работает корректно
    }
}
