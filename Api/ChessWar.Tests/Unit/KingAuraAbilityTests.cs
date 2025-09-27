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
        var owner = new Player("P1", new List<Piece>());
        var king = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4), owner);
        var ally = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, new Position(6, 4), owner); // дистанция Чебышёва = 2

        var baseAtk = ally.ATK;

        var cfg = _TestConfig.CreateProvider();
        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnSvc = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var enemy = new Player("P2", new List<Piece>());
        var session = new GameSession(owner, enemy);
        session.StartGame();
        var turn = session.GetCurrentTurn();

        turn.SelectPiece(king);
        
        var action = new TurnAction("Move", king.Id.ToString(), new Position(4, 4));
        turn.AddAction(action);
        
        turnSvc.EndTurn(turn);

        ally.ATK.Should().Be(baseAtk + 1);

        ally.Position = new Position(0, 0); // далеко
        var nextTurn = session.GetCurrentTurn();
        nextTurn.SelectPiece(king);
        turnSvc.EndTurn(nextTurn);

        ally.ATK.Should().Be(baseAtk);
    }
}





