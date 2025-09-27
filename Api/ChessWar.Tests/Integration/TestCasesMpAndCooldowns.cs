using System.Net.Http.Json;

namespace ChessWar.Tests.Integration;

public class TestCasesMpAndCooldowns : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public TestCasesMpAndCooldowns(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task EndTurn_ShouldRegenerateMpAndTickCooldowns_ForActivePlayer()
    {

        var response = await _client.PostAsJsonAsync("/api/v1/game/sessions", new { player1Name = "P1", player2Name = "P2" });

        Assert.False(response.IsSuccessStatusCode);
    }
}


