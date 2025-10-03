using FluentAssertions;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Services.GameLogic;

namespace ChessWar.Tests.Unit;

public class EvolutionServiceTests
{
    [Fact]
    public void GetPossibleEvolutions_Pawn_ShouldReturnKnightAndBishop()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());

        var evolutions = evolutionManager.GetPossibleEvolutions(PieceType.Pawn);

        evolutions.Should().HaveCount(2);
        evolutions.Should().Contain(PieceType.Knight);
        evolutions.Should().Contain(PieceType.Bishop);
    }

    [Fact]
    public void GetPossibleEvolutions_Knight_ShouldReturnRook()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());

        var evolutions = evolutionManager.GetPossibleEvolutions(PieceType.Knight);

        evolutions.Should().HaveCount(1);
        evolutions.Should().Contain(PieceType.Rook);
    }

    [Fact]
    public void GetPossibleEvolutions_Bishop_ShouldReturnRook()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());

        var evolutions = evolutionManager.GetPossibleEvolutions(PieceType.Bishop);

        evolutions.Should().HaveCount(1);
        evolutions.Should().Contain(PieceType.Rook);
    }

    [Fact]
    public void GetPossibleEvolutions_Rook_ShouldReturnQueen()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());

        var evolutions = evolutionManager.GetPossibleEvolutions(PieceType.Rook);

        evolutions.Should().HaveCount(1);
        evolutions.Should().Contain(PieceType.Queen);
    }

    [Fact]
    public void GetPossibleEvolutions_Queen_ShouldReturnEmpty()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());

        var evolutions = evolutionManager.GetPossibleEvolutions(PieceType.Queen);

        evolutions.Should().BeEmpty();
    }

    [Fact]
    public void GetPossibleEvolutions_King_ShouldReturnEmpty()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());

        var evolutions = evolutionManager.GetPossibleEvolutions(PieceType.King);

        evolutions.Should().BeEmpty();
    }

    [Fact]
    public void CanEvolve_PawnWithEnoughXP_ShouldReturnTrue()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        TestHelpers.AddXP(pawn, 20);

        var canEvolve = evolutionManager.CanEvolve(pawn);

        canEvolve.Should().BeTrue();
    }

    [Fact]
    public void CanEvolve_PawnWithoutEnoughXP_ShouldReturnFalse()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        TestHelpers.AddXP(pawn, 10);

        var canEvolve = evolutionManager.CanEvolve(pawn);

        canEvolve.Should().BeFalse();
    }

    [Fact]
    public void CanEvolve_Queen_ShouldReturnFalse()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var queen = TestHelpers.CreatePiece(PieceType.Queen, Team.Elves, 0, 0);

        var canEvolve = evolutionManager.CanEvolve(queen);

        canEvolve.Should().BeFalse();
    }

    [Fact]
    public void MeetsEvolutionRequirements_PawnToKnight_ShouldReturnTrue()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        TestHelpers.AddXP(pawn, 20);

        var meetsRequirements = evolutionManager.MeetsEvolutionRequirements(pawn, PieceType.Knight);

        meetsRequirements.Should().BeTrue();
    }

    [Fact]
    public void MeetsEvolutionRequirements_PawnToQueen_ShouldReturnFalse()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        TestHelpers.AddXP(pawn, 20);

        var meetsRequirements = evolutionManager.MeetsEvolutionRequirements(pawn, PieceType.Queen);

        meetsRequirements.Should().BeFalse();
    }

    [Fact]
    public void EvolvePiece_PawnToKnight_ShouldReturnKnight()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        TestHelpers.AddXP(pawn, 20);

        var evolvedPiece = evolutionManager.EvolvePiece(pawn, PieceType.Knight);

        evolvedPiece.Should().NotBeNull();
        evolvedPiece.Type.Should().Be(PieceType.Knight);
        evolvedPiece.Team.Should().Be(Team.Elves);
        evolvedPiece.Position.Should().Be(pawn.Position);
    }

    [Fact]
    public void EvolvePiece_PawnToBishop_ShouldReturnBishop()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, 0, 0);
        TestHelpers.AddXP(pawn, 20);

        var evolvedPiece = evolutionManager.EvolvePiece(pawn, PieceType.Bishop);

        evolvedPiece.Should().NotBeNull();
        evolvedPiece.Type.Should().Be(PieceType.Bishop);
        evolvedPiece.Team.Should().Be(Team.Elves);
        evolvedPiece.Position.Should().Be(pawn.Position);
    }

    [Fact]
    public void EvolvePiece_ShouldLogEvolution()
    {
        var evolutionManager = new EvolutionService(_TestConfig.CreateProvider());
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        TestHelpers.AddXP(pawn, 20);

        var evolvedPiece = evolutionManager.EvolvePiece(pawn, PieceType.Knight);

        var history = evolutionManager.GetEvolutionHistory();
        history.Should().HaveCount(1);
        history[0].PieceId.Should().Be(pawn.Id);
        history[0].NewType.Should().Be(PieceType.Knight);
    }
}
