using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using Microsoft.Extensions.Logging;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Сервис ИИ для выполнения ходов
/// </summary>
public class AIService : IAIService
{
    private readonly IActionGenerator _actionGenerator;
    private readonly IActionSelector _actionSelector;
    private readonly IActionExecutor _actionExecutor;
    private readonly ILogger<AIService> _logger;

    public AIService(
        IActionGenerator actionGenerator,
        IActionSelector actionSelector,
        IActionExecutor actionExecutor,
        ILogger<AIService> logger)
    {
        _actionGenerator = actionGenerator ?? throw new ArgumentNullException(nameof(actionGenerator));
        _actionSelector = actionSelector ?? throw new ArgumentNullException(nameof(actionSelector));
        _actionExecutor = actionExecutor ?? throw new ArgumentNullException(nameof(actionExecutor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Выполнить ход ИИ
    /// </summary>
    public bool MakeAiTurn(GameSession session)
    {
        var turn = session.GetCurrentTurn();
        var active = turn.ActiveParticipant;

        if (!active.IsAI)
        {
            _logger.LogWarning("Active participant is not AI");
            return false;
        }

        var boardPieces = session.GetBoard().GetAlivePiecesByTeam(active.Team);
        if (boardPieces.Count == 0)
        {
            _logger.LogWarning("No pieces available for AI turn");
            return false;
        }

        try
        {
            var availableActions = _actionGenerator.GenerateActions(session, turn, active);
            
            if (!availableActions.Any())
            {
                _logger.LogWarning("No available actions found");
                return false;
            }

            var selectedActions = _actionSelector.SelectActions(session, turn, active, availableActions);
            
            if (!selectedActions.Any())
            {
                _logger.LogWarning("No actions selected");
                return false;
            }

            var success = _actionExecutor.ExecuteActions(session, turn, selectedActions);
            
            if (!success)
            {
                _logger.LogWarning("Failed to execute turn");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception during AI turn");
            return false;
        }
    }

    /// <summary>
    /// Выполнить ход ИИ асинхронно
    /// </summary>
    public async Task<bool> MakeAiTurnAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => MakeAiTurn(session), cancellationToken);
    }

    /// <summary>
    /// Обучить ИИ на основе результатов игры
    /// </summary>
    public void LearnFromGame(GameSession session, object result)
    {
        _logger.LogInformation("Learning from game result: {Result}", result);
    }
}
