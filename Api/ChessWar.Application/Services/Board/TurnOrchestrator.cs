using ChessWar.Application.Interfaces.Board; using ChessWar.Application.Interfaces.AI; using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.GameLogic; using ChessWar.Domain.Interfaces.DataAccess; using ChessWar.Domain.Interfaces.Configuration;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Координатор ходов - управляет завершением ходов и связанными операциями
/// </summary>
public class TurnOrchestrator : ITurnOrchestrator
{
    private readonly ITurnCompletionService _turnCompletionService;
    private readonly IAITurnService _aiService;
    private readonly IGameStateService _gameStateService;
    private readonly IGameNotificationService _notificationService;
    private readonly IGameSessionRepository _sessionRepository;
    private readonly IBalanceConfigProvider _configProvider;
    private readonly ILogger<TurnOrchestrator> _logger;

    public TurnOrchestrator(
        ITurnCompletionService turnCompletionService,
        IAITurnService aiService,
        IGameStateService gameStateService,
        IGameNotificationService notificationService,
        IGameSessionRepository sessionRepository,
        IBalanceConfigProvider configProvider,
        ILogger<TurnOrchestrator> logger)
    {
        _turnCompletionService = turnCompletionService ?? throw new ArgumentNullException(nameof(turnCompletionService));
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _gameStateService = gameStateService ?? throw new ArgumentNullException(nameof(gameStateService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidateGameSession(gameSession);
            LogTurnStart(gameSession);
            
            var config = _configProvider.GetActive();
            if (config.PlayerMana.MandatoryAction)
            {
                var currentTurn = gameSession.GetCurrentTurn();
                if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
                {
                    throw new InvalidOperationException("хотя бы одного действия");
                }
            }
            
                var activePlayerBeforeTurn = gameSession.GetCurrentTurn().ActiveParticipant;
                
                await ProcessCurrentTurn(gameSession, cancellationToken);
                
                var activePlayerAfterTurn = gameSession.GetCurrentTurn().ActiveParticipant;
                
                if (ShouldCallAIForNextTurn(gameSession, activePlayerAfterTurn) && 
                    activePlayerBeforeTurn != activePlayerAfterTurn)
                {
                    await ExecuteAITurn(gameSession, cancellationToken);
                }
            
            await CheckAndHandleGameCompletion(gameSession, cancellationToken);
            await SaveGameState(gameSession, cancellationToken);
            
            LogTurnCompletion(gameSession);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in EndTurnAsync: {Message}", ex.Message);
            throw;
        }
    }

    private void ValidateGameSession(GameSession gameSession)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));
    }

    private void LogTurnStart(GameSession gameSession)
    {
        _logger.LogInformation("Starting EndTurnAsync for session: {SessionId}", gameSession.Id);
    }

    private async Task ProcessCurrentTurn(GameSession gameSession, CancellationToken cancellationToken)
    {
        await _turnCompletionService.EndTurnAsync(gameSession, cancellationToken);
    }


    private async Task ExecuteAITurn(GameSession gameSession, CancellationToken cancellationToken)
    {
        try
        {
            var aiSuccess = await _aiService.MakeAiTurnAsync(gameSession, cancellationToken);
            
            if (aiSuccess)
            {
                await _notificationService.NotifyAiMoveAsync(gameSession.Id, 
                    new { Turn = gameSession.GetCurrentTurn().Number }, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI turn threw exception: {Message}", ex.Message);
        }
    }

    private async Task CheckAndHandleGameCompletion(GameSession gameSession, CancellationToken cancellationToken)
    {
        var result = _gameStateService.CheckVictory(gameSession);
        if (result.HasValue)
        {
            _logger.LogInformation("Game completed with result: {Result}", result.Value);
            gameSession.CompleteGame(result.Value);
            
            await _notificationService.NotifyGameEndedAsync(gameSession.Id, 
                result.Value.ToString(), 
                $"Игра завершена: {result.Value}", 
                cancellationToken);
        }
    }

    private async Task SaveGameState(GameSession gameSession, CancellationToken cancellationToken)
    {
        await _sessionRepository.SaveAsync(gameSession, cancellationToken);
    }

    private void LogTurnCompletion(GameSession gameSession)
    {
        _logger.LogInformation("Turn completed successfully");
        _logger.LogInformation("Final active player: {PlayerName} (ID: {PlayerId})", 
            gameSession.GetCurrentTurn().ActiveParticipant.Name, 
            gameSession.GetCurrentTurn().ActiveParticipant.Id);
    }


    private bool ShouldCallAIForNextTurn(GameSession gameSession, Participant activePlayerAfterTurnCompletion)
    {
        return (gameSession.Mode == "AI" || gameSession.TutorialSessionId != null) && 
               activePlayerAfterTurnCompletion == gameSession.Player2 &&
               activePlayerAfterTurnCompletion is ChessWar.Domain.Entities.AI;
    }
}
