using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;
using ChessWar.Infrastructure.Services;
using ChessWar.Domain.Interfaces.DataAccess;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для проверки правил эволюции фигур
/// </summary>
public class EvolutionRulesTests
{
    private readonly IEvolutionService _evolutionService;
    private readonly IBalanceConfigProvider _configProvider;

    public EvolutionRulesTests()
    {
        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        _configProvider = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);
        _evolutionService = new ChessWar.Domain.Services.GameLogic.EvolutionService(_configProvider);
    }

    [Fact]
    public void Pawn_WithEnoughXP_CanEvolveToKnightOrBishop()
    {
        // Arrange
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        pawn.XP = 20; // Достаточно XP для эволюции

        // Act & Assert
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(PieceType.Pawn);
        
        Assert.Contains(PieceType.Knight, possibleEvolutions);
        Assert.Contains(PieceType.Bishop, possibleEvolutions);
        Assert.DoesNotContain(PieceType.Rook, possibleEvolutions);
        Assert.DoesNotContain(PieceType.Queen, possibleEvolutions);
    }

    [Fact]
    public void Pawn_WithEnoughXP_CannotEvolveToQueen()
    {
        // Arrange
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        pawn.XP = 20; // Достаточно XP для эволюции

        // Act & Assert
        var canEvolveToQueen = _evolutionService.MeetsEvolutionRequirements(pawn, PieceType.Queen);
        
        Assert.False(canEvolveToQueen);
    }

    [Fact]
    public void Pawn_WithEnoughXP_CannotEvolveToRook()
    {
        // Arrange
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        pawn.XP = 20; // Достаточно XP для эволюции

        // Act & Assert
        var canEvolveToRook = _evolutionService.MeetsEvolutionRequirements(pawn, PieceType.Rook);
        
        Assert.False(canEvolveToRook);
    }

    [Fact]
    public void Knight_WithEnoughXP_CanEvolveToRook()
    {
        // Arrange
        var knight = new Piece(PieceType.Knight, Team.Orcs, new Position(0, 0));
        knight.XP = 40; // Достаточно XP для эволюции

        // Act & Assert
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(PieceType.Knight);
        
        Assert.Contains(PieceType.Rook, possibleEvolutions);
        Assert.DoesNotContain(PieceType.Queen, possibleEvolutions);
    }

    [Fact]
    public void Bishop_WithEnoughXP_CanEvolveToRook()
    {
        // Arrange
        var bishop = new Piece(PieceType.Bishop, Team.Orcs, new Position(0, 0));
        bishop.XP = 40; // Достаточно XP для эволюции

        // Act & Assert
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(PieceType.Bishop);
        
        Assert.Contains(PieceType.Rook, possibleEvolutions);
        Assert.DoesNotContain(PieceType.Queen, possibleEvolutions);
    }

    [Fact]
    public void Rook_WithEnoughXP_CanEvolveToQueen()
    {
        // Arrange
        var rook = new Piece(PieceType.Rook, Team.Orcs, new Position(0, 0));
        rook.XP = 60; // Достаточно XP для эволюции

        // Act & Assert
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(PieceType.Rook);
        
        Assert.Contains(PieceType.Queen, possibleEvolutions);
    }

    [Fact]
    public void Queen_CannotEvolve()
    {
        // Arrange
        var queen = new Piece(PieceType.Queen, Team.Orcs, new Position(0, 0));
        queen.XP = 100; // Много XP, но не должно помочь

        // Act & Assert
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(PieceType.Queen);
        
        Assert.Empty(possibleEvolutions);
    }

    [Fact]
    public void King_CannotEvolve()
    {
        // Arrange
        var king = new Piece(PieceType.King, Team.Orcs, new Position(0, 0));
        king.XP = 100; // Много XP, но не должно помочь

        // Act & Assert
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(PieceType.King);
        
        Assert.Empty(possibleEvolutions);
    }

    [Fact]
    public void Pawn_WithInsufficientXP_CannotEvolve()
    {
        // Arrange
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        pawn.XP = 19; // Недостаточно XP для эволюции

        // Act & Assert
        var canEvolveToKnight = _evolutionService.MeetsEvolutionRequirements(pawn, PieceType.Knight);
        var canEvolveToBishop = _evolutionService.MeetsEvolutionRequirements(pawn, PieceType.Bishop);
        
        Assert.False(canEvolveToKnight);
        Assert.False(canEvolveToBishop);
    }

    [Fact]
    public void Pawn_OnLastRank_CanEvolveImmediately()
    {
        // Arrange - пешка орков на последней линии (y=0)
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        pawn.XP = 0; // Без XP, но на последней линии

        // Act & Assert
        var canEvolve = _evolutionService.CanEvolve(pawn);
        Assert.True(canEvolve);
    }

    [Fact]
    public void Pawn_OnLastRank_CanEvolveToKnightOrBishop()
    {
        // Arrange - пешка эльфов на последней линии (y=7)
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 7));
        pawn.XP = 0; // Без XP, но на последней линии

        // Act & Assert
        // На последней линии пешка может эволюционировать без XP через CanEvolve + EvolvePiece
        _evolutionService.CanEvolve(pawn).Should().BeTrue();
        var evolvedKnight = _evolutionService.EvolvePiece(pawn, PieceType.Knight);
        evolvedKnight.Type.Should().Be(PieceType.Knight);
        evolvedKnight.Position.Should().Be(pawn.Position);
    }
}
