using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Configuration;

namespace ChessWar.Application.Services;

/// <summary>
/// Сервис для работы со статистикой
/// </summary>
public class StatsService : IStatsService
{
    public Task<PlayerStatsDto> GetPlayerStatsAsync(string playerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Player stats retrieval is not implemented yet. This is where you'll implement statistics functionality.");
    }

    public Task UpdatePlayerStatsAsync(string playerId, GameResultDto result, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Player stats update is not implemented yet. This is where you'll implement statistics tracking.");
    }
}

