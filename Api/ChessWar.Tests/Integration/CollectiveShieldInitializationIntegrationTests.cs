using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционные тесты для проверки инициализации щитов при создании игры
/// </summary>
public class CollectiveShieldInitializationIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public CollectiveShieldInitializationIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    /// <summary>
    /// Проверяет, что при создании новой игры щиты всех фигур инициализируются
    /// </summary>
    [Fact]
    public async Task CreateGameSession_ShouldInitializeShields_ForAllPieces()
    {
       
        var createDto = new CreateGameSessionDto
        {
            Player1Name = "Player1",
            Player2Name = "Player2",
            Mode = "LocalCoop"
        };

       
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

       
        var getResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var gameSession = await getResp.Content.ReadFromJsonAsync<GameSessionDto>();
        gameSession.Should().NotBeNull();

       
        var allPieces = gameSession!.Player1.Pieces.Concat(gameSession.Player2.Pieces);
        
        foreach (var piece in allPieces)
        {
            if (piece.Type.ToString() == "King")
            {
                piece.ShieldHp.Should().BeGreaterThan(0, $"Король {piece.Id} должен иметь щит > 0");
            }
            else
            {
                piece.ShieldHp.Should().BeGreaterThan(0, $"Фигура {piece.Id} должна иметь щит > 0");
                piece.NeighborCount.Should().BeGreaterOrEqualTo(0, $"Фигура {piece.Id} должна иметь количество соседей >= 0");
            }
        }
    }
}