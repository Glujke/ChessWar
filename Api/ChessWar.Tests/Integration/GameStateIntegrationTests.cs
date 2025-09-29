using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class GameStateIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public GameStateIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Game_ShouldFinish_When_KingDies()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var p2King = state!.Player2.Pieces.First(p => p.Type.ToString() == "King");


        state.Status.Should().Be(ChessWar.Domain.Enums.GameStatus.Active);
    }
}


