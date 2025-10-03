using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Менеджер состояния игры - только сохранение состояния
/// </summary>
public class GameStateManager : IGameStateManager
{
    private readonly IGameStateService _gameStateService;
    private readonly IGameSessionRepository _sessionRepository;
    private readonly ILogger<GameStateManager> _logger;

    public GameStateManager(
        IGameStateService gameStateService,
        IGameSessionRepository sessionRepository,
        ILogger<GameStateManager> logger)
    {
        _gameStateService = gameStateService ?? throw new ArgumentNullException(nameof(gameStateService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GameResult?> CheckAndHandleGameCompletionAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        try
        {
            var result = _gameStateService.CheckVictory(gameSession);
            if (result.HasValue)
            {
                _logger.LogInformation("Game completed with result: {Result} for session {SessionId}", result.Value, gameSession.Id);
                gameSession.CompleteGame(result.Value);

                await _sessionRepository.SaveAsync(gameSession, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking game completion for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            throw;
        }
    }

    public async Task SaveGameStateAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        try
        {
            await _sessionRepository.SaveAsync(gameSession, cancellationToken);
            _logger.LogDebug("Game state saved for session {SessionId}", gameSession.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving game state for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            throw;
        }
    }

    public async Task<bool> IsGameCompletedAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        return gameSession.Status == Domain.Enums.GameStatus.Finished;
    }
}
