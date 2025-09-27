using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class KingQueenAbilitiesIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public KingQueenAbilitiesIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RoyalCommand_FirstUse_Ok_SecondUse_BadRequest_When_OnCooldown()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        stateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var king = state!.Player1.Pieces.Find(p => p.Type.ToString() == "King");
        var ally = state.Player1.Pieces.Find(p => p.Id != king!.Id);

        var req = new { pieceId = king!.Id.ToString(), abilityName = "RoyalCommand", target = new { x = ally!.Position.X, y = ally.Position.Y } };

        var first = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", req);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", req);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Resurrection_ShouldReviveAlly_ReturnOk()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var queen = state!.Player1.Pieces.Find(p => p.Type.ToString() == "Queen");
        var ally = state.Player1.Pieces.Find(p => p.Id != queen!.Id);

        var req = new { pieceId = queen!.Id.ToString(), abilityName = "Resurrection", target = new { x = ally!.Position.X, y = ally.Position.Y } };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", req);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}


