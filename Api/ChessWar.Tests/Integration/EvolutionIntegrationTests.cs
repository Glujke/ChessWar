using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class EvolutionIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public EvolutionIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Pawn_Reaches_LastRank_ShouldAllow_EvolutionEndpoint()
    {
        // Arrange: создать сессию
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var pawn = state!.Player1.Pieces.First(p => p.Type.ToString() == "Pawn");

        // Симулируем перемещение пешки на последнюю линию (упростим: запрашиваем эволюцию напрямую)
        var evolveReq = new { pieceId = pawn.Id.ToString(), targetType = "Knight" };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/evolve", evolveReq);

        // Ожидаем 200 после реализации эндпоинта (сейчас падает в красную фазу)
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}


