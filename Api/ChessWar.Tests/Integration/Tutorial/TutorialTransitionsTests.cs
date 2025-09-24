using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace ChessWar.Tests.Integration.Tutorial;

public class TutorialTransitionsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TutorialTransitionsTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StartTutorial_ShouldStartWith_Battle1_Preset()
    {
        var startResponse = await _client.PostAsJsonAsync("/api/v1/game/tutorial", new
        {
            playerId = "tutorial-test-player",
            showHints = false
        });
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await startResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var gameSessionId = root.GetProperty("gameSessionId").GetString();
        gameSessionId.Should().NotBeNullOrEmpty();

        var gameResp = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        gameResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Transition_To_Battle2_ShouldRequire_Player1Victory()
    {
        // Start tutorial → Battle1
        var startResponse = await _client.PostAsJsonAsync("/api/v1/game/tutorial", new
        {
            playerId = "tutorial-transition-test",
            showHints = false
        });
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await startResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var gameSessionId = root.GetProperty("gameSessionId").GetString();
        gameSessionId.Should().NotBeNullOrEmpty();

        // Имитация поражения игрока на Battle1 → пытаемся перейти
        var completeLose = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/complete?result=Player2Victory", null);
        // Ожидаем 409/BadRequest (логика должна запретить авто-переход вперёд)
        completeLose.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Transition_To_Boss_ShouldRequire_Player1Victory_On_Battle2()
    {
        // Start tutorial → Battle1
        var startResponse = await _client.PostAsJsonAsync("/api/v1/game/tutorial", new
        {
            playerId = "tutorial-boss-guard",
            showHints = false
        });
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await startResponse.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var gameSessionId = doc.RootElement.GetProperty("gameSessionId").GetString();

        // Завершаем Battle1 победой игрока → ожидаем корректный переход на Battle2
        var completeWin1 = await _client.PostAsync($"/api/v1/gamesession/{gameSessionId}/complete?result=Player1Victory", null);
        completeWin1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Берём новый gameSessionId из ответа перехода
        var transJson = await completeWin1.Content.ReadAsStringAsync();
        using var transDoc = JsonDocument.Parse(transJson);
        var nextId = transDoc.RootElement.GetProperty("gameSessionId").GetString();
        nextId.Should().NotBeNullOrEmpty();

        // Завершаем Battle2 поражением игрока → переход на Boss ДОЛЖЕН БЫТЬ ЗАПРЕЩЁН
        var completeLose2 = await _client.PostAsync($"/api/v1/gamesession/{nextId}/complete?result=Player2Victory", null);
        completeLose2.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);
    }
}


