using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.TurnManagement;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Единый процессор ходов - обрабатывает ВСЕ типы ходов (игрок + AI)
/// </summary>
public class TurnProcessor : ITurnProcessor
{
    private readonly ITurnService _turnService;
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IBalanceConfigProvider _configProvider;
    private readonly IAITurnService _aiTurnService;
    private readonly ICollectiveShieldService _collectiveShieldService;
    private readonly ILogger<TurnProcessor> _logger;

    public TurnProcessor(
        ITurnService turnService,
        IGameSessionRepository sessionRepository,
        IBalanceConfigProvider configProvider,
        IAITurnService aiTurnService,
        ICollectiveShieldService collectiveShieldService,
        ILogger<TurnProcessor> logger)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _aiTurnService = aiTurnService ?? throw new ArgumentNullException(nameof(aiTurnService));
        _collectiveShieldService = collectiveShieldService ?? throw new ArgumentNullException(nameof(collectiveShieldService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessTurnPhaseAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        try
        {
            _logger.LogInformation("Processing turn phase for session {SessionId}", gameSession.Id);

            var currentTurn = gameSession.GetCurrentTurn();
            var activePlayer = currentTurn.ActiveParticipant;

            await ProcessPlayerTurn(gameSession, activePlayer, cancellationToken);

            var config = _configProvider.GetActive();
            gameSession.EndCurrentTurnWithManaRestore(config.PlayerMana.ManaRegenPerTurn);

            var currentActivePlayer = gameSession.GetCurrentTurn().ActiveParticipant;

            if (ShouldProcessAITurn(gameSession, currentActivePlayer))
            {
                await ProcessAITurn(gameSession, currentActivePlayer, cancellationToken);
                gameSession.EndCurrentTurnWithManaRestore(config.PlayerMana.ManaRegenPerTurn);
            }

            await TickCooldownsForAllPieces(gameSession);

            await RegenerateKingShields(gameSession);

            await _sessionRepository.SaveAsync(gameSession, cancellationToken);

            _logger.LogInformation("Turn phase completed for session {SessionId}", gameSession.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing turn phase for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            throw;
        }
    }

    private async Task ProcessPlayerTurn(GameSession gameSession, Participant player, CancellationToken cancellationToken)
    {
        if (player == null)
            return;

        _logger.LogInformation("Processing turn for player {PlayerId} in session {SessionId}", player.Id, gameSession.Id);

        var currentTurn = gameSession.GetCurrentTurn();

        _logger.LogInformation("Processing player turn for {PlayerId}, actions count: {ActionsCount}",
            player.Id, currentTurn.Actions?.Count ?? 0);

        if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
        {
            var activePlayerPieces = player.Pieces ?? new List<ChessWar.Domain.Entities.Piece>();
            var hasAnyAction = activePlayerPieces.Any(p => p.IsAlive &&
                (_turnService.GetAvailableMoves(gameSession, currentTurn, p).Any() || _turnService.GetAvailableAttacks(currentTurn, p).Any()));

            var noMana = player.MP <= 0 || currentTurn.RemainingMP <= 0;

            _logger.LogInformation("Turn validation: hasAnyAction={HasAnyAction}, noMana={NoMana}, playerMP={PlayerMP}, remainingMP={RemainingMP}",
                hasAnyAction, noMana, player.MP, currentTurn.RemainingMP);

            if (hasAnyAction && !noMana)
            {
                _logger.LogWarning("Turn validation failed: player has available actions but no actions performed");
                throw new InvalidOperationException("Для завершения хода требуется выполнение хотя бы одного действия.");
            }
        }

        _turnService.EndTurn(currentTurn);

        _logger.LogInformation("Player turn completed for {PlayerId} in session {SessionId}", player.Id, gameSession.Id);
    }

    private async Task ProcessAITurn(GameSession gameSession, Participant aiPlayer, CancellationToken cancellationToken)
    {
        if (aiPlayer == null || !(aiPlayer is ChessWar.Domain.Entities.AI))
            return;

        _logger.LogInformation("Processing AI turn for {PlayerId} in session {SessionId}", aiPlayer.Id, gameSession.Id);

        var aiSuccess = await _aiTurnService.MakeAiTurnAsync(gameSession, cancellationToken);

        if (!aiSuccess)
        {
            _logger.LogWarning("AI turn failed for {PlayerId} in session {SessionId}", aiPlayer.Id, gameSession.Id);
        }

        var currentTurn = gameSession.GetCurrentTurn();

        if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
        {
            currentTurn.AddAction(new ChessWar.Domain.ValueObjects.TurnAction("Pass", string.Empty, null));
        }

        _turnService.EndTurn(currentTurn);

        _logger.LogInformation("AI turn completed for {PlayerId} in session {SessionId}", aiPlayer.Id, gameSession.Id);
    }

    private async Task TickCooldownsForAllPieces(GameSession gameSession)
    {
        var allPieces = gameSession.GetAllPieces();

        foreach (var piece in allPieces.Where(p => p.IsAlive))
        {
            foreach (var cooldownKey in piece.AbilityCooldowns.Keys.ToList())
            {
                if (piece.AbilityCooldowns[cooldownKey] > 0)
                {
                    piece.AbilityCooldowns[cooldownKey]--;
                }
            }
        }

        _logger.LogInformation("Cooldowns ticked for all pieces in session {SessionId}", gameSession.Id);
    }

    private bool ShouldProcessAITurn(GameSession gameSession, Participant currentPlayer)
    {
        return (gameSession.Mode == "AI" || gameSession.TutorialSessionId != null) &&
               currentPlayer is ChessWar.Domain.Entities.AI;
    }

    private Participant GetNextPlayer(GameSession gameSession, Participant currentPlayer)
    {
        return currentPlayer == gameSession.Player1 ? gameSession.Player2 : gameSession.Player1;
    }

    private async Task RegenerateKingShields(GameSession gameSession)
    {
        var allPieces = gameSession.Board.Pieces.ToList();
        var kings = allPieces.Where(p => p.Type == PieceType.King).ToList();
        
        foreach (var king in kings)
        {
            var allyPieces = allPieces.Where(p => p.Owner == king.Owner && p.Id != king.Id).ToList();
            _collectiveShieldService.RegenerateKingShield(king, allyPieces);
        }
    }
}
