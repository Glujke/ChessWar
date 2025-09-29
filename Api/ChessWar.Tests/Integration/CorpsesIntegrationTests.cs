using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class CorpsesIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public CorpsesIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task DeadPieces_ShouldRemainInPlayerCollection_ButNotOnBoard()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var initialState = await GetGameState(session!.Id);
        var initialPlayer1PiecesCount = initialState.Player1.Pieces.Count;
        var initialPlayer2PiecesCount = initialState.Player2.Pieces.Count;

        var target = initialState.Player2.Pieces.First();
        
        
        var finalState = await GetGameState(session.Id);

        finalState.Player1.Pieces.Count.Should().Be(initialPlayer1PiecesCount);
        finalState.Player2.Pieces.Count.Should().Be(initialPlayer2PiecesCount);

        finalState.Player1.Pieces.Should().OnlyContain(p => p.HP > 0, "Все фигуры должны быть живы");
        finalState.Player2.Pieces.Should().OnlyContain(p => p.HP > 0, "Все фигуры должны быть живы");

        finalState.Player1.Pieces.Should().OnlyContain(p => p.Position != null, "Все живые фигуры должны иметь позиции");
        finalState.Player2.Pieces.Should().OnlyContain(p => p.Position != null, "Все живые фигуры должны иметь позиции");
    }

    private async Task<GameSessionDto> GetGameState(Guid sessionId)
    {
        var response = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<GameSessionDto>();
        state.Should().NotBeNull();
        return state!;
    }

    [Fact]
    public async Task DeadPieces_ShouldNotOccupyCells_OnGameBoard()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var state = await GetGameState(session!.Id);

        var allPieces = state.Player1.Pieces.Concat(state.Player2.Pieces).ToList();
        
        allPieces.Should().NotBeEmpty("В игровой сессии должны быть фигуры");
        
        allPieces.Should().OnlyContain(p => p.HP > 0, "Все фигуры должны быть живы");
        
        allPieces.Should().OnlyContain(p => p.Position != null, "Все фигуры должны иметь позиции");
    }

}
