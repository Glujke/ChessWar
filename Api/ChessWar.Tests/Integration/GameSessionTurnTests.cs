using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class GameSessionTurnTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public GameSessionTurnTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateSession_Then_EndTurn_ShouldReturn200_AndAffectMpAndCooldowns()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);

        Assert.True(createResponse.IsSuccessStatusCode);

        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        Assert.NotNull(session);
        session!.Status.Should().Be(Domain.Enums.GameStatus.Active);

        var beforeResp = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
        beforeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var before = await beforeResp.Content.ReadFromJsonAsync<GameSessionDto>();
        before.Should().NotBeNull();

        var pawn = before!.Player1.Pieces.First(p => p.Type == Domain.Enums.PieceType.Pawn);
        var moveRequest = new
        {
            pieceId = pawn.Id.ToString(),
            targetPosition = new { X = pawn.Position.X, Y = pawn.Position.Y + 1 }
        };
        await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/move", moveRequest);

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{session!.Id}/turn/end", null);

        Assert.Equal(HttpStatusCode.OK, endTurnResponse.StatusCode);

        var afterResp = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
        afterResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = await afterResp.Content.ReadFromJsonAsync<GameSessionDto>();
        after.Should().NotBeNull();

        after!.Player1.Pieces.Should().NotBeEmpty();
        after.Player2.Pieces.Should().NotBeEmpty();

        after.Player1.MP.Should().Be(before!.Player1.MP - 1 + 10);
        after.Player1.MP.Should().BeLessOrEqualTo(after.Player1.MaxMP);
    }
}


