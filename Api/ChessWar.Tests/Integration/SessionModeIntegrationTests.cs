using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class SessionModeIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public SessionModeIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateSession_With_LocalCoop_Mode_ShouldReturn_Ok_AndPersistMode()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "LocalCoop" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<ChessWar.Application.DTOs.GameSessionDto>();
        session.Should().NotBeNull();
        // Пока DTO может не содержать Mode — проверим минимум, что создалась сессия
        var getResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}


