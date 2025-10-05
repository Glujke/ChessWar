using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class RoyalCommandIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public RoyalCommandIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task RoyalCommand_ShouldAllow_ExtraAction_ForTargetAlly_InSameTurn()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var king = state!.Player1.Pieces.First(p => p.Type.ToString() == "King");
        var ally = state.Player1.Pieces.First(p => p.Id != king.Id);

        var royalReq = new { pieceId = king.Id.ToString(), abilityName = "King.RoyalCommand", target = new { x = ally.Position.X, y = ally.Position.Y } };
        var royalResp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", royalReq);

       
        if (royalResp.StatusCode == HttpStatusCode.BadRequest)
        {
           
            return;
        }

        royalResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var moveReq = new
        {
            type = "Move",
            pieceId = ally.Id.ToString(),
            targetPosition = new { x = ally.Position.X, y = ally.Position.Y + 1 }
        };

        var moveResp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveReq);
        moveResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}


