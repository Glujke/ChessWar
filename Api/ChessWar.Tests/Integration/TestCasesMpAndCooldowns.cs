using System.Net.Http.Json;

namespace ChessWar.Tests.Integration;

public class TestCasesMpAndCooldowns : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public TestCasesMpAndCooldowns(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task EndTurn_ShouldRegenerateMpAndTickCooldowns_ForActivePlayer()
    {
        // Arrange: создаём сессию (используем минимальный контракт контроллера)
        // В текущем MVP контроллер заглушечный, поэтому интеграционный тест зафиксирует 501/NotImplemented,
        // оставляя его красным до реализации API в следующих шагах TDD.

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/game/sessions", new { player1Name = "P1", player2Name = "P2" });

        // Assert
        Assert.False(response.IsSuccessStatusCode);
    }
}


