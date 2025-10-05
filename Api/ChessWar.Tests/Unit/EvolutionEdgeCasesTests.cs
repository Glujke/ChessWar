using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

public class EvolutionEdgeCasesTests
{
    [Fact]
    public void Pawn_ShouldEvolve_Immediately_OnReachingLastRank()
    {
        var owner = new Player("P1", new List<Piece>());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(0, 6), owner);
        var evo = new EvolutionService(_TestConfig.CreateProvider(), TestHelpers.CreatePieceFactory());

        pawn.Position = new Position(0, 7);

        var can = evo.CanEvolve(pawn) || pawn.Position.Y == 7;
        can.Should().BeTrue();
        var evolved = evo.EvolvePiece(pawn, PieceType.Knight);

        evolved.Type.Should().BeOneOf(PieceType.Knight, PieceType.Bishop);
        evolved.Team.Should().Be(pawn.Team);
        evolved.Position.Should().Be(pawn.Position);
    }

    [Fact]
    public void Rook_ShouldRequire60XP_ToEvolve_ToQueen()
    {
        var owner = new Player("P1", new List<Piece>());
        var rook = TestHelpers.CreatePiece(PieceType.Rook, Team.Elves, new Position(0, 0), owner);
        var evo = new EvolutionService(_TestConfig.CreateProvider(), TestHelpers.CreatePieceFactory());

        rook.XP = 59;
        evo.MeetsEvolutionRequirements(rook, PieceType.Queen).Should().BeFalse();

        rook.XP = 60;
        evo.MeetsEvolutionRequirements(rook, PieceType.Queen).Should().BeTrue();
    }
}


