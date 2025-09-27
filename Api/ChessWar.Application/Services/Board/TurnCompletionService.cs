using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.TurnManagement; using ChessWar.Domain.Interfaces.DataAccess; using ChessWar.Domain.Interfaces.Configuration;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис завершения ходов
/// </summary>
public class TurnCompletionService : ITurnCompletionService
{
    private readonly ITurnService _turnService;
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IBalanceConfigProvider _configProvider;
    private readonly ILogger<TurnCompletionService> _logger;

    public TurnCompletionService(
        ITurnService turnService,
        IGameSessionRepository sessionRepository,
        IBalanceConfigProvider configProvider,
        ILogger<TurnCompletionService> logger)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        var currentTurn = gameSession.GetCurrentTurn();
        var activePlayerBefore = currentTurn.ActiveParticipant;
        
        _logger.LogInformation("=== TURN COMPLETION DEBUG ===");
        
        if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
        {
            var activePlayerPieces = currentTurn.ActiveParticipant?.Pieces ?? new List<ChessWar.Domain.Entities.Piece>();
            var hasAnyAction = activePlayerPieces.Any(p => p.IsAlive &&
                (_turnService.GetAvailableMoves(gameSession, currentTurn, p).Any() || _turnService.GetAvailableAttacks(currentTurn, p).Any()));

            var noMana = currentTurn.ActiveParticipant?.MP <= 0 || currentTurn.RemainingMP <= 0;

            if (!hasAnyAction || noMana)
            {
                currentTurn.AddAction(new ChessWar.Domain.ValueObjects.TurnAction("Pass", string.Empty, null));
            }
        }

        _turnService.EndTurn(currentTurn);

        var config = _configProvider.GetActive();
        
        gameSession.EndCurrentTurnWithManaRestore(config.PlayerMana.ManaRegenPerTurn);
        
        var newTurn = gameSession.GetCurrentTurn();
        var activePlayerAfter = newTurn.ActiveParticipant;

        _logger.LogInformation("=== END TURN COMPLETION DEBUG ===");

        await _sessionRepository.SaveAsync(gameSession, cancellationToken);
    }
}
