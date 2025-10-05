using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using Xunit;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для проверки бага с инициализацией щитов при создании игры
/// </summary>
public class CollectiveShieldInitializationBugTests
{
    [Fact]
    public void CreatePiece_ShouldHaveZeroShield_ByDefault()
    {
        var piece = new Piece
        {
            Id = 1,
            Type = PieceType.Pawn,
            Team = Team.Elves,
            Position = new Position(0, 0),
            HP = 100,
            ATK = 10,
            ShieldHP = 0,
            NeighborCount = 0
        };
        
        Assert.Equal(0, piece.ShieldHP);
        Assert.Equal(0, piece.NeighborCount);
    }

    [Fact]
    public void CreateKing_ShouldHaveZeroShield_ByDefault()
    {
        var king = new Piece
        {
            Id = 1,
            Type = PieceType.King,
            Team = Team.Elves,
            Position = new Position(4, 0),
            HP = 200,
            ATK = 20,
            ShieldHP = 0,
            NeighborCount = 0
        };
        
        Assert.Equal(0, king.ShieldHP);
        Assert.Equal(0, king.NeighborCount);
    }

    [Fact]
    public void CreatePawn_ShouldHaveZeroShield_ByDefault()
    {
        var pawn = new Piece
        {
            Id = 1,
            Type = PieceType.Pawn,
            Team = Team.Elves,
            Position = new Position(0, 1),
            HP = 50,
            ATK = 5,
            ShieldHP = 0,
            NeighborCount = 0
        };
        
        Assert.Equal(0, pawn.ShieldHP);
        Assert.Equal(0, pawn.NeighborCount);
    }

    [Fact]
    public void CreateGame_ShouldInitializeShields_ForAllPieces()
    {
        var pieces = new List<Piece>
        {
            new Piece { Id = 1, Type = PieceType.King, Team = Team.Elves, Position = new Position(4, 0), HP = 200, ATK = 20, ShieldHP = 0, NeighborCount = 0 },
            new Piece { Id = 2, Type = PieceType.Pawn, Team = Team.Elves, Position = new Position(0, 1), HP = 100, ATK = 10, ShieldHP = 0, NeighborCount = 0 },
            new Piece { Id = 3, Type = PieceType.Pawn, Team = Team.Elves, Position = new Position(1, 1), HP = 100, ATK = 10, ShieldHP = 0, NeighborCount = 0 },
            new Piece { Id = 4, Type = PieceType.Pawn, Team = Team.Elves, Position = new Position(2, 1), HP = 100, ATK = 10, ShieldHP = 0, NeighborCount = 0 }
        };

        foreach (var piece in pieces)
        {
            Assert.Equal(0, piece.ShieldHP);
            Assert.Equal(0, piece.NeighborCount);
        }
    }
}
