using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class GameSessionAbilityTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public GameSessionAbilityTests(TestWebApplicationFactory factory) : base(factory) { }

    private static (int pieceId, int x, int y) FindPieceByType(GameSessionDto session, string type)
    {
        var all = session.Player1.Pieces.Concat(session.Player2.Pieces);
        var p = all.FirstOrDefault(p => p.Type.ToString() == type);
        p.Should().NotBeNull();
        return (p!.Id, p.Position.X, p.Position.Y);
    }

    [Fact]
    public async Task Ability_LightArrow_ShouldSpendMp_AndSetCooldown()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        var beforeResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        beforeResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var before = await beforeResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var (pieceId, _, _) = FindPieceByType(before!, "Bishop");

        var abilityReq = new
        {
            pieceId = pieceId.ToString(),
            abilityName = "LightArrow",
            target = new { x = 4, y = 4 }
        };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", abilityReq);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var afterResp = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
        afterResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = await afterResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var pieceAfter = after!.Player1.Pieces.First(p => p.Id == pieceId);
        after!.Player1.MP.Should().BeLessThan(before!.Player1.MP); // Игрок потратил ману
        pieceAfter.AbilityCooldowns.GetValueOrDefault("LightArrow").Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Ability_ShouldFail_WhenInsufficientMp()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var pawn = state!.Player1.Pieces.First(p => p.Type.ToString() == "Pawn");

        var abilityReq = new
        {
            pieceId = pawn.Id.ToString(),
            abilityName = "LightArrow", // не её способность, ожидаем отказ по правилам/MP
            target = new { x = pawn.Position.X, y = pawn.Position.Y + 1 }
        };

        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", abilityReq);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Ability_ShouldFail_WhenOnCooldown()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var queen = state!.Player1.Pieces.First(p => p.Type.ToString() == "Queen");

        var req = new { pieceId = queen.Id.ToString(), abilityName = "MagicExplosion", target = new { x = queen.Position.X + 1, y = queen.Position.Y } };
        var first = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", req);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var second = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", req);
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}


