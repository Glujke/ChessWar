using ChessWar.Application.Services;
using ChessWar.Application.DTOs;

namespace ChessWar.Tests.Unit;

public class StatsServiceTests
{
    private readonly StatsService _service;

    public StatsServiceTests()
    {
        _service = new StatsService();
    }

    [Fact]
    public async Task GetPlayerStatsAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var playerId = "player123";

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => 
            _service.GetPlayerStatsAsync(playerId));
    }

    [Fact]
    public async Task UpdatePlayerStatsAsync_ShouldThrowNotImplementedException()
    {
        // Arrange
        var playerId = "player123";
        var result = new GameResultDto
        {
            GameMode = "Tutorial",
            Result = "Win",
            DurationMinutes = 15,
            CompletedAt = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => 
            _service.UpdatePlayerStatsAsync(playerId, result));
    }
}
