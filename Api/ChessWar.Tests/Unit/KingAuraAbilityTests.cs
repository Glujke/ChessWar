using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using ChessWar.Tests.Helpers;

namespace ChessWar.Tests.Unit;

public class KingAuraAbilityTests
{
    [Fact]
    public void Aura_ShouldIncreaseAllyAttack_By1_WithinRadius3_AndRemoveOutside()
    {
        // Arrange
        var owner = new Player("P1", new List<Piece>());
        var king = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4), owner);
        var ally = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, new Position(6, 4), owner); // дистанция Чебышёва = 2

        var baseAtk = ally.ATK;

        // Имитация эффекта ауры в конце StartTurn / в начале расчетов — пока проверим EndTurn обработкой
        var cfg = _TestConfig.CreateProvider();
        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnSvc = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var enemy = new Player("P2", new List<Piece>());
        var session = new GameSession(owner, enemy);
        session.StartGame();
        var turn = session.GetCurrentTurn();

        // Act: в конце хода аура должна примениться (упростим логику, проверяя к окончанию тика)
        turn.SelectPiece(king);
        
        // Добавляем действие в ход (обязательное требование)
        var action = new TurnAction("Move", king.Id.ToString(), new Position(4, 4));
        turn.AddAction(action);
        
        turnSvc.EndTurn(turn);

        // Assert: в радиусе 3 у союзника ATK +1
        ally.ATK.Should().Be(baseAtk + 1);

        // Переместим союзника за радиус, аура должна сняться к концу следующего тика
        ally.Position = new Position(0, 0); // далеко
        var nextTurn = session.GetCurrentTurn();
        nextTurn.SelectPiece(king);
        turnSvc.EndTurn(nextTurn);

        ally.ATK.Should().Be(baseAtk);
    }
}





