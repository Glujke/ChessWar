using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ChessWar.Tests.Integration;

public class PieceServiceIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    private readonly IPieceService _pieceService;

    public PieceServiceIntegrationTests(TestWebApplicationFactory factory) : base(factory)
    {
        _pieceService = _scope.ServiceProvider.GetRequiredService<IPieceService>();
    }

    [Fact]
    public async Task CreatePiece_WithValidData_ShouldPersistToDatabase()
    {
        var type = PieceType.Pawn;
        var team = Team.Elves;
        var position = new Position(1, 1);

        var result = await _pieceService.CreatePieceAsync(type, team, position);

        result.Should().NotBeNull();
        result.Type.Should().Be(type);
        result.Team.Should().Be(team);
        result.Position.Should().Be(position);
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPieceById_AfterCreating_ShouldReturnCreatedPiece()
    {
        var type = PieceType.Knight;
        var team = Team.Orcs;
        var position = new Position(2, 2);
        var createdPiece = await _pieceService.CreatePieceAsync(type, team, position);

        var result = await _pieceService.GetPieceByIdAsync(createdPiece.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(createdPiece.Id);
        result.Type.Should().Be(type);
        result.Team.Should().Be(team);
        result.Position.Should().Be(position);
    }

    [Fact]
    public async Task GetAllPieces_AfterCreatingMultiple_ShouldReturnAllPieces()
    {
        await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        await _pieceService.CreatePieceAsync(PieceType.Knight, Team.Orcs, new Position(2, 2));
        await _pieceService.CreatePieceAsync(PieceType.Bishop, Team.Elves, new Position(3, 3));

        var result = await _pieceService.GetAllPiecesAsync();

        result.Should().HaveCount(3);
        result.Should().Contain(p => p.Type == PieceType.Pawn && p.Team == Team.Elves);
        result.Should().Contain(p => p.Type == PieceType.Knight && p.Team == Team.Orcs);
        result.Should().Contain(p => p.Type == PieceType.Bishop && p.Team == Team.Elves);
    }

    [Fact]
    public async Task GetPiecesByTeam_ShouldReturnOnlyTeamPieces()
    {
        await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        await _pieceService.CreatePieceAsync(PieceType.Knight, Team.Orcs, new Position(2, 2));
        await _pieceService.CreatePieceAsync(PieceType.Bishop, Team.Elves, new Position(3, 3));

        var elvesPieces = await _pieceService.GetPiecesByTeamAsync(Team.Elves);
        var orcsPieces = await _pieceService.GetPiecesByTeamAsync(Team.Orcs);

        elvesPieces.Should().HaveCount(2);
        elvesPieces.Should().AllSatisfy(p => p.Team.Should().Be(Team.Elves));
        
        orcsPieces.Should().HaveCount(1);
        orcsPieces.Should().AllSatisfy(p => p.Team.Should().Be(Team.Orcs));
    }

    [Fact]
    public async Task UpdatePiecePosition_ShouldUpdatePositionInDatabase()
    {
        var piece = await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        var newPosition = new Position(3, 3);

        var result = await _pieceService.UpdatePiecePositionAsync(piece.Id, newPosition);

        result.Position.Should().Be(newPosition);
        
        var retrievedPiece = await _pieceService.GetPieceByIdAsync(piece.Id);
        retrievedPiece!.Position.Should().Be(newPosition);
    }

    [Fact]
    public async Task UpdatePieceStats_ShouldUpdateStatsInDatabase()
    {
        var piece = await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));

        var result = await _pieceService.UpdatePieceStatsAsync(piece.Id, hp: 15, atk: 5, mp: 8, xp: 10);

        result.HP.Should().Be(15);
        result.ATK.Should().Be(5);
        result.XP.Should().Be(10);
        
        var retrievedPiece = await _pieceService.GetPieceByIdAsync(piece.Id);
        retrievedPiece!.HP.Should().Be(15);
        retrievedPiece.ATK.Should().Be(5);
        retrievedPiece.XP.Should().Be(10);
    }

    [Fact]
    public async Task DeletePiece_ShouldRemoveFromDatabase()
    {
        var piece = await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));

        await _pieceService.DeletePieceAsync(piece.Id);

        var result = await _pieceService.GetPieceByIdAsync(piece.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsPositionFree_WithFreePosition_ShouldReturnTrue()
    {
        var position = new Position(1, 1);

        var result = await _pieceService.IsPositionFreeAsync(position);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsPositionFree_WithOccupiedPosition_ShouldReturnFalse()
    {
        var position = new Position(1, 1);
        await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, position);

        var result = await _pieceService.IsPositionFreeAsync(position);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreatePiece_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        var position = new Position(1, 1);
        await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, position);

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _pieceService.CreatePieceAsync(PieceType.Knight, Team.Orcs, position));
    }

    [Fact]
    public async Task UpdatePiecePosition_WithOccupiedPosition_ShouldThrowInvalidOperationException()
    {
        var piece1 = await _pieceService.CreatePieceAsync(PieceType.Pawn, Team.Elves, new Position(1, 1));
        var piece2 = await _pieceService.CreatePieceAsync(PieceType.Knight, Team.Orcs, new Position(2, 2));

        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _pieceService.UpdatePiecePositionAsync(piece1.Id, piece2.Position));
    }

}
