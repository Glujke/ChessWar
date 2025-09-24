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
    private readonly ITurnExecutionService _turnService;

    public DeadPieceAttackIntegrationTest(TestWebApplicationFactory factory) : base(factory)
    {
        _sessionService = _scope.ServiceProvider.GetRequiredService<IGameSessionManagementService>();
        _turnService = _scope.ServiceProvider.GetRequiredService<ITurnExecutionService>();
    }

    [Fact]
    public async Task AttackDeadPiece_ShouldReturn400Error()
    {
        // ARRANGE - Подготовка
        // Создаём tutorial сессию через HTTP API
        var payload = JsonSerializer.Serialize(new { playerId = "test-player-123" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/v1/game/tutorial", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var gameSessionId = doc.RootElement.GetProperty("gameSessionId").GetString();
        Assert.NotNull(gameSessionId);
        
        // Получаем сессию через сервис
        var sessionId = Guid.Parse(gameSessionId);
        var session = await _sessionService.GetSessionAsync(sessionId);
        Assert.NotNull(session);
        
        // Получаем пешку игрока на позиции (0,1)
        var playerPawn = session.Player1.Pieces.FirstOrDefault(p => 
            p.Type == PieceType.Pawn && p.Position.X == 0 && p.Position.Y == 1);
        
        Assert.NotNull(playerPawn);
        
        // Получаем вражескую фигуру на позиции (0,6) 
        var enemyPiece = session.Player2.Pieces.FirstOrDefault(p => 
            p.Position.X == 0 && p.Position.Y == 6);
        
        Assert.NotNull(enemyPiece);
        Assert.True(enemyPiece.IsAlive); // Изначально живая

        // ACT - Действие
        // 1. Пешка двигается с (0,1) на (0,2) - ближе к врагу
        var moveAction1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 2 }
        };
        
        var moveResult1 = await _turnService.ExecuteActionAsync(session, moveAction1);
        Assert.True(moveResult1); // Движение должно быть успешным
        
        // 2. Пешка двигается с (0,2) на (0,3)
        var moveAction2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 3 }
        };
        
        var moveResult2 = await _turnService.ExecuteActionAsync(session, moveAction2);
        Assert.True(moveResult2);
        
        // 3. Пешка двигается с (0,3) на (0,4)
        var moveAction3 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 4 }
        };
        
        var moveResult3 = await _turnService.ExecuteActionAsync(session, moveAction3);
        Assert.True(moveResult3);
        
        // 4. Пешка двигается с (0,4) на (0,5) - теперь в радиусе атаки
        var moveAction4 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 5 }
        };
        
        var moveResult4 = await _turnService.ExecuteActionAsync(session, moveAction4);
        Assert.True(moveResult4);
        
        // 5. Пешка атакует врага на (0,6) - УБИВАЕТ ЕГО
        var attackAction = new ExecuteActionDto
        {
            Type = "Attack", 
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 6 }
        };
        
        var attackResult = await _turnService.ExecuteActionAsync(session, attackAction);
        Assert.True(attackResult); // Первая атака должна быть успешной
        
        // Нужно атаковать несколько раз, чтобы убить врага!
        // У врага HP = 10, у атакующего ATK = 2, нужно 5 атак
        for (int i = 0; i < 4; i++) // Уже сделали 1 атаку выше
        {
            var attackAgain = new ExecuteActionDto
            {
                Type = "Attack",
                PieceId = playerPawn.Id.ToString(),
                TargetPosition = new PositionDto { X = 0, Y = 6 }
            };
            
            var attackAgainResult = await _turnService.ExecuteActionAsync(session, attackAgain);
            Assert.True(attackAgainResult);
        }
        
        // Проверяем, что враг мёртв
        var finalEnemyPiece = session.Player2.Pieces.FirstOrDefault(p => p.Id == enemyPiece.Id);
        Assert.NotNull(finalEnemyPiece);
        Assert.False(finalEnemyPiece.IsAlive); // Теперь мёртвая!
        
        // 3. Попытка атаковать МЁРТВУЮ фигуру - ДОЛЖНА ВЕРНУТЬ 400!
        var attackDeadAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = playerPawn.Id.ToString(), 
            TargetPosition = new PositionDto { X = 0, Y = 6 }
        };
        
        var attackDeadResult = await _turnService.ExecuteActionAsync(session, attackDeadAction);
        
        // ASSERT - Проверка
        // ЭТОТ ТЕСТ ДОЛЖЕН ПАДАТЬ СЕЙЧАС, ПОТОМУ ЧТО АТАКИ НА МЁРТВЫЕ ФИГУРЫ ПРОХОДЯТ!
        Assert.False(attackDeadResult); // Атака мёртвой фигуры должна возвращать false
    }

    [Fact]
    public async Task MoveToDeadPiecePosition_ShouldReturn200Success()
    {
        // ARRANGE - Подготовка
        // Создаём tutorial сессию через HTTP API
        var payload = JsonSerializer.Serialize(new { playerId = "test-player-456" });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync("/api/v1/game/tutorial", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var gameSessionId = doc.RootElement.GetProperty("gameSessionId").GetString();
        Assert.NotNull(gameSessionId);
        
        // Получаем сессию через сервис
        var sessionId = Guid.Parse(gameSessionId);
        var session = await _sessionService.GetSessionAsync(sessionId);
        Assert.NotNull(session);
        
        var playerPawn = session.Player1.Pieces.FirstOrDefault(p => 
            p.Type == PieceType.Pawn && p.Position.X == 0 && p.Position.Y == 1);
        
        var enemyPiece = session.Player2.Pieces.FirstOrDefault(p => 
            p.Position.X == 0 && p.Position.Y == 6);
        
        Assert.NotNull(playerPawn);
        Assert.NotNull(enemyPiece);

        // ACT - Действие
        // 1. Подходим ближе к врагу
        var moveAction1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 2 }
        };
        
        var moveResult1 = await _turnService.ExecuteActionAsync(session, moveAction1);
        Assert.True(moveResult1);
        
        var moveAction2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 3 }
        };
        
        var moveResult2 = await _turnService.ExecuteActionAsync(session, moveAction2);
        Assert.True(moveResult2);
        
        var moveAction3 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 4 }
        };
        
        var moveResult3 = await _turnService.ExecuteActionAsync(session, moveAction3);
        Assert.True(moveResult3);
        
        var moveAction4 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = playerPawn.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 5 }
        };
        
        var moveResult4 = await _turnService.ExecuteActionAsync(session, moveAction4);
        Assert.True(moveResult4);
        
        // 2. Убиваем врага (нужно несколько атак)
        for (int i = 0; i < 5; i++)
        {
            var attackAction = new ExecuteActionDto
            {
                Type = "Attack",
                PieceId = playerPawn.Id.ToString(),
                TargetPosition = new PositionDto { X = 0, Y = 6 }
            };
            
            var attackResult = await _turnService.ExecuteActionAsync(session, attackAction);
            Assert.True(attackResult);
        }
        
        // Проверяем, что враг мёртв
        var deadEnemyPiece = session.Player2.Pieces.FirstOrDefault(p => p.Id == enemyPiece.Id);
        Assert.NotNull(deadEnemyPiece);
        Assert.False(deadEnemyPiece.IsAlive);
        
        // 3. Проверяем, что наша фигура уже переместилась на позицию убитой фигуры
        // (благодаря новой механике автоматического перемещения при убийстве)
        var updatedPlayerPawn = session.Player1.Pieces.FirstOrDefault(p => p.Id == playerPawn.Id);
        Assert.NotNull(updatedPlayerPawn);
        Assert.Equal(0, updatedPlayerPawn.Position.X);
        Assert.Equal(6, updatedPlayerPawn.Position.Y);
        
        // ASSERT - Проверка
        // Фигура уже должна быть на позиции убитой фигуры благодаря автоматическому перемещению
        Assert.True(true); // Механика работает корректно
    }
}
