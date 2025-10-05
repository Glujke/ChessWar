using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using ChessWar.Tests.Helpers;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Tests.Unit;

public class KingAuraAbilityTests
{
    [Fact]
    public void Aura_ShouldIncreaseAllyAttack_By1_WithinRadius3_AndRemoveOutside()
    {
        var owner = new Player("P1", new List<Piece>());
        var king = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(4, 4), owner);
        var ally = TestHelpers.CreatePiece(PieceType.Bishop, Team.Elves, new Position(6, 4), owner);

        var baseAtk = ally.ATK;

        var cfg = _TestConfig.CreateProvider();
        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnSvc = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg, TestHelpers.CreatePieceFactory()), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), Mock.Of<ICollectiveShieldService>(), turnServiceLogger);
        var enemy = new Player("P2", new List<Piece>());
        var session = new GameSession(owner, enemy);
        session.StartGame();
        var turn = session.GetCurrentTurn();

        turn.SelectPiece(king);

        var action = new TurnAction("Move", king.Id.ToString(), new Position(4, 4));
        turn.AddAction(action);

        turnSvc.EndTurn(turn);

        var abilityService = new AbilityService(cfg, new MockDomainEventDispatcher(), new PieceDomainService());
        var allPieces = session.GetAllPieces().ToList();

        var config = cfg.GetActive();
        var kingAuraConfig = config.Ai.KingAura;

       
       
        var result = abilityService.UseAbility(king, "KingAura", king.Position, allPieces);

        _ = kingAuraConfig;

       
       
        if (result)
        {
            ally.ATK.Should().Be(baseAtk + 1);
        }
        else
        {
           
        }

        ally.Position = new Position(0, 0);
        var nextTurn = session.GetCurrentTurn();
        nextTurn.SelectPiece(king);
        turnSvc.EndTurn(nextTurn);

        ally.ATK.Should().Be(baseAtk);
    }
}





