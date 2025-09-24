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

public class RookFortressAbilityTests
{
    [Fact]
    public void Fortress_ShouldDoubleHp_Until_EndOfTurn_Then_Revert()
    {
        // Arrange
        var owner = new Player("P1", new List<Piece>());
        owner.SetMana(50, 50); // У игрока есть мана
        var rook = TestHelpers.CreatePiece(PieceType.Rook, Team.Elves, new Position(1, 1), owner);
        var eventDispatcher = new Mock<ChessWar.Domain.Events.IDomainEventDispatcher>();
        var pieceDomainService = new Mock<ChessWar.Domain.Interfaces.GameLogic.IPieceDomainService>();
        
        // Настраиваем мок для обработки cooldown и GetMaxHP
        pieceDomainService
            .Setup(x => x.SetAbilityCooldown(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<Piece, string, int>((piece, ability, cooldown) => piece.AbilityCooldowns[ability] = cooldown);
        pieceDomainService
            .Setup(x => x.GetMaxHP(It.IsAny<PieceType>()))
            .Returns<PieceType>(type => type switch
            {
                PieceType.Rook => 15,
                _ => 10
            });
        
        var svc = new AbilityService(_TestConfig.CreateProvider(), eventDispatcher.Object, pieceDomainService.Object);
        var baseHp = rook.HP;

        // Act: применяем Крепость
        var ok = svc.UseAbility(rook, "Fortress", rook.Position, owner.Pieces);

        // Assert: HP удвоен и есть КД
        ok.Should().BeTrue();
        rook.HP.Should().BeGreaterThan(baseHp);
        rook.AbilityCooldowns.GetValueOrDefault("Fortress").Should().BeGreaterThan(0);

        // Эмулируем конец хода активного игрока
        var cfg = _TestConfig.CreateProvider();
        var movementRulesLogger = Mock.Of<ILogger<MovementRulesService>>();
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        var turnSvc = new TurnService(new MovementRulesService(movementRulesLogger), new AttackRulesService(), new EvolutionService(cfg), cfg, new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        var enemy = new Player("P2", new List<Piece>());
        var dummySession = new GameSession(owner, enemy);
        dummySession.StartGame();
        var turn = dummySession.GetCurrentTurn();
        turn.SelectPiece(rook);
        
        // Добавляем действие в ход (обязательное требование)
        var action = new TurnAction("Move", rook.Id.ToString(), new Position(0, 1));
        turn.AddAction(action);
        
        turnSvc.EndTurn(turn);

        // Ожидаем снятие баффа (возврат HP к базовому максимуму)
        rook.HP.Should().BeLessOrEqualTo(TestHelpers.GetMaxHP(rook));
    }
}





