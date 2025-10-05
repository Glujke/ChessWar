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
        var name = "TestPlayer";
        var pieces = CreateTestPieces();

        var player = new Player(name, pieces);

        player.Should().NotBeNull();
        player.Name.Should().Be(name);
        player.Pieces.Should().BeEquivalentTo(pieces);
        player.Victories.Should().Be(0);
        player.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreatePlayer_WithNullName_ShouldThrowArgumentNullException()
    {
        string? name = null;
        var pieces = CreateTestPieces();

        var action = () => new Player(name!, pieces);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("name");
    }

    [Fact]
    public void CreatePlayer_WithEmptyName_ShouldThrowArgumentException()
    {
        var name = "";
        var pieces = CreateTestPieces();

        var action = () => new Player(name, pieces);
        action.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void CreatePlayer_WithNullPieces_ShouldThrowArgumentNullException()
    {
        var name = "TestPlayer";
        List<Piece>? pieces = null;

        var action = () => new Player(name, pieces!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("pieces");
    }

    [Fact]
    public void AddVictory_ShouldIncrementVictories()
    {
        var player = CreateTestPlayer();

        player.AddVictory();

        player.Victories.Should().Be(1);
    }

    [Fact]
    public void AddVictory_MultipleTimes_ShouldIncrementCorrectly()
    {
        var player = CreateTestPlayer();

        player.AddVictory();
        player.AddVictory();
        player.AddVictory();

        player.Victories.Should().Be(3);
    }


    [Fact]
    public void GetPiecesByType_ShouldReturnCorrectPieces()
    {
        var player = CreateTestPlayer();

        var pawns = player.GetPiecesByType(PieceType.Pawn);
        var kings = player.GetPiecesByType(PieceType.King);

        pawns.Should().HaveCount(8);
        pawns.Should().AllSatisfy(piece => piece.Type.Should().Be(PieceType.Pawn));

        kings.Should().HaveCount(1);
        kings.Should().AllSatisfy(piece => piece.Type.Should().Be(PieceType.King));
    }

    [Fact]
    public void GetAlivePieces_ShouldReturnOnlyAlivePieces()
    {
        var player = CreateTestPlayer();
        var pieces = player.Pieces.ToList();

        TestHelpers.TakeDamage(pieces[0], 1000);
        TestHelpers.TakeDamage(pieces[1], 1000);

        var alivePieces = player.GetAlivePieces();

        alivePieces.Should().HaveCount(7);
        alivePieces.Should().AllSatisfy(piece => piece.IsAlive.Should().BeTrue());
    }

    [Fact]
    public void GetPiecesByTeam_ShouldReturnCorrectPieces()
    {
        var player = CreateTestPlayer();

        var elvesPieces = player.GetPiecesByTeam(Team.Elves);
        var orcsPieces = player.GetPiecesByTeam(Team.Orcs);

        elvesPieces.Should().HaveCount(9);
        elvesPieces.Should().AllSatisfy(piece => piece.Team.Should().Be(Team.Elves));

        orcsPieces.Should().BeEmpty();
    }

    [Fact]
    public void AddPiece_ShouldAddPieceToCollection()
    {
        var player = CreateTestPlayer();
        var newPiece = new Piece(PieceType.Knight, Team.Elves, new Position(0, 0));
        var initialCount = player.Pieces.Count;

        player.AddPiece(newPiece);

        player.Pieces.Should().HaveCount(initialCount + 1);
        player.Pieces.Should().Contain(newPiece);
    }

    [Fact]
    public void RemovePiece_ShouldRemovePieceFromCollection()
    {
        var player = CreateTestPlayer();
        var pieceToRemove = player.Pieces.First();
        var initialCount = player.Pieces.Count;

        player.RemovePiece(pieceToRemove);

        player.Pieces.Should().HaveCount(initialCount - 1);
        player.Pieces.Should().NotContain(pieceToRemove);
    }

    [Fact]
    public void RemovePiece_WithNonExistentPiece_ShouldNotThrow()
    {
        var player = CreateTestPlayer();
        var nonExistentPiece = new Piece(PieceType.Knight, Team.Orcs, new Position(0, 0));

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

        for (int i = 0; i < 8; i++)
        {
            pieces.Add(TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, i, 1));
        }

        pieces.Add(TestHelpers.CreatePiece(PieceType.King, Team.Elves, 4, 0));

        return pieces;
    }
}
