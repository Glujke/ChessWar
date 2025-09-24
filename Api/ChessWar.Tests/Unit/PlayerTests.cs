using FluentAssertions;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit;

public class PlayerTests
{
    [Fact]
    public void CreatePlayer_WithValidData_ShouldCreatePlayer()
    {
        // Arrange
        var name = "TestPlayer";
        var pieces = CreateTestPieces();

        // Act
        var player = new Player(name, pieces);

        // Assert
        player.Should().NotBeNull();
        player.Name.Should().Be(name);
        player.Pieces.Should().BeEquivalentTo(pieces);
        player.Victories.Should().Be(0);
        player.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreatePlayer_WithNullName_ShouldThrowArgumentNullException()
    {
        // Arrange
        string? name = null;
        var pieces = CreateTestPieces();

        // Act & Assert
        var action = () => new Player(name!, pieces);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void CreatePlayer_WithEmptyName_ShouldThrowArgumentException()
    {
        // Arrange
        var name = "";
        var pieces = CreateTestPieces();

        // Act & Assert
        var action = () => new Player(name, pieces);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void CreatePlayer_WithNullPieces_ShouldThrowArgumentNullException()
    {
        // Arrange
        var name = "TestPlayer";
        List<Piece>? pieces = null;

        // Act & Assert
        var action = () => new Player(name, pieces!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("pieces");
    }

    [Fact]
    public void AddVictory_ShouldIncrementVictories()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        player.AddVictory();

        // Assert
        player.Victories.Should().Be(1);
    }

    [Fact]
    public void AddVictory_MultipleTimes_ShouldIncrementCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        player.AddVictory();
        player.AddVictory();
        player.AddVictory();

        // Assert
        player.Victories.Should().Be(3);
    }


    [Fact]
    public void GetPiecesByType_ShouldReturnCorrectPieces()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var pawns = player.GetPiecesByType(PieceType.Pawn);
        var kings = player.GetPiecesByType(PieceType.King);

        // Assert
        pawns.Should().HaveCount(8);
        pawns.Should().AllSatisfy(piece => piece.Type.Should().Be(PieceType.Pawn));
        
        kings.Should().HaveCount(1);
        kings.Should().AllSatisfy(piece => piece.Type.Should().Be(PieceType.King));
    }

    [Fact]
    public void GetAlivePieces_ShouldReturnOnlyAlivePieces()
    {
        // Arrange
        var player = CreateTestPlayer();
        var pieces = player.Pieces.ToList();
        
        // Убиваем несколько фигур
        TestHelpers.TakeDamage(pieces[0], 1000);
        TestHelpers.TakeDamage(pieces[1], 1000);

        // Act
        var alivePieces = player.GetAlivePieces();

        // Assert
        alivePieces.Should().HaveCount(7); // 9 - 2 = 7
        alivePieces.Should().AllSatisfy(piece => piece.IsAlive.Should().BeTrue());
    }

    [Fact]
    public void GetPiecesByTeam_ShouldReturnCorrectPieces()
    {
        // Arrange
        var player = CreateTestPlayer();

        // Act
        var elvesPieces = player.GetPiecesByTeam(Team.Elves);
        var orcsPieces = player.GetPiecesByTeam(Team.Orcs);

        // Assert
        elvesPieces.Should().HaveCount(9);
        elvesPieces.Should().AllSatisfy(piece => piece.Team.Should().Be(Team.Elves));
        
        orcsPieces.Should().BeEmpty();
    }

    [Fact]
    public void AddPiece_ShouldAddPieceToCollection()
    {
        // Arrange
        var player = CreateTestPlayer();
        var newPiece = new Piece(PieceType.Knight, Team.Elves, new Position(0, 0));
        var initialCount = player.Pieces.Count;

        // Act
        player.AddPiece(newPiece);

        // Assert
        player.Pieces.Should().HaveCount(initialCount + 1);
        player.Pieces.Should().Contain(newPiece);
    }

    [Fact]
    public void RemovePiece_ShouldRemovePieceFromCollection()
    {
        // Arrange
        var player = CreateTestPlayer();
        var pieceToRemove = player.Pieces.First();
        var initialCount = player.Pieces.Count;

        // Act
        player.RemovePiece(pieceToRemove);

        // Assert
        player.Pieces.Should().HaveCount(initialCount - 1);
        player.Pieces.Should().NotContain(pieceToRemove);
    }

    [Fact]
    public void RemovePiece_WithNonExistentPiece_ShouldNotThrow()
    {
        // Arrange
        var player = CreateTestPlayer();
        var nonExistentPiece = new Piece(PieceType.Knight, Team.Orcs, new Position(0, 0));

        // Act & Assert
        var action = () => player.RemovePiece(nonExistentPiece);
        action.Should().NotThrow();
    }

    private static Player CreateTestPlayer()
    {
        var pieces = CreateTestPieces();
        return new Player("TestPlayer", pieces);
    }

    private static List<Piece> CreateTestPieces()
    {
        var pieces = new List<Piece>();
        
        // Создаём 8 пешек
        for (int i = 0; i < 8; i++)
        {
            pieces.Add(TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, i, 1));
        }
        
        // Создаём короля
        pieces.Add(TestHelpers.CreatePiece(PieceType.King, Team.Elves, 4, 0));
        
        return pieces;
    }
}
