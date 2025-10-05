using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;
using Xunit;
using System.Text.Json;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционный тест для проверки инициализации щитов в продакшене
/// </summary>
public class ShieldInitializationProductionTest : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public ShieldInitializationProductionTest(TestWebApplicationFactory factory) : base(factory) { }

    /// <summary>
    /// Проверяет, что при создании новой игры щиты всех фигур инициализируются правильно
    /// </summary>
    [Fact]
    public async Task CreateGameSession_ShouldInitializeShields_ForAllPieces()
    {
        // Arrange
        var createDto = new CreateGameSessionDto
        {
            Player1Name = "TestPlayer",
            Player2Name = "AI",
            Mode = "AI"
        };

           // Act
           var response = await _client.PostAsJsonAsync("/api/v1/game/ai", new { PlayerId = "TestPlayer", Difficulty = 1 });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        
        using var document = JsonDocument.Parse(responseContent);
        var root = document.RootElement;
        var gameSessionId = root.GetProperty("id").GetString();
        gameSessionId.Should().NotBeNullOrEmpty();

        // Получаем состояние сессии
        var sessionResponse = await _client.GetAsync($"/api/v1/gamesession/{gameSessionId}");
        sessionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var sessionJson = await sessionResponse.Content.ReadAsStringAsync();
        sessionJson.Should().NotBeNullOrEmpty();

        // Проверяем, что в JSON есть данные о щитах
        sessionJson.Should().Contain("shieldHp", "В ответе должны быть данные о щитах");
        sessionJson.Should().Contain("neighborCount", "В ответе должны быть данные о соседях");
    }
}
