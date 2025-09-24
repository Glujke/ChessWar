using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

public class GameStateServiceTests
{
    [Fact]
    public void ShouldDetect_Victory_When_OpponentKingDead()
    {
        var p1 = new Player("P1", new List<Piece>());
        var p2 = new Player("P2", new List<Piece>());
        var k1 = TestHelpers.CreatePiece(PieceType.King, Team.Elves, new Position(0,0), p1);
        var k2 = TestHelpers.CreatePiece(PieceType.King, Team.Orcs, new Position(1,1), p2);

        var session = new GameSession(p1, p2);
        var gs = new GameStateService();

        TestHelpers.TakeDamage(k2, 1000);
        var result = gs.CheckVictory(session);

        result.Should().Be(GameResult.Player1Victory);
    }
}


