using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Планировщик AI ходов - только планирование AI ходов
/// </summary>
public class AITurnScheduler : IAITurnScheduler
{
    private readonly IAITurnService _aiService;
    private readonly ILogger<AITurnScheduler> _logger;

    public AITurnScheduler(
        IAITurnService aiService,
        ILogger<AITurnScheduler> logger)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ScheduleAITurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (!ShouldScheduleAI(gameSession))
        {
            _logger.LogDebug("AI turn not needed for session {SessionId}", gameSession.Id);
            return false;
        }

        try
        {
            _logger.LogInformation("Scheduling AI turn for session {SessionId}", gameSession.Id);

            var success = await _aiService.MakeAiTurnAsync(gameSession, cancellationToken);

            if (success)
            {
                _logger.LogInformation("AI turn scheduled successfully for session {SessionId}", gameSession.Id);
            }
            else
            {
                _logger.LogWarning("AI turn scheduling failed for session {SessionId}", gameSession.Id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling AI turn for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            return false;
        }
    }

    public bool ShouldScheduleAI(GameSession gameSession)
    {
        if (gameSession == null)
            return false;

        var currentTurn = gameSession.GetCurrentTurn();
        var activePlayer = currentTurn.ActiveParticipant;

        return (gameSession.Mode == "AI" || gameSession.TutorialSessionId != null) &&
               activePlayer == gameSession.Player1 &&
               gameSession.Player2 is ChessWar.Domain.Entities.AI;
    }

    public async Task<bool> ExecuteAITurnAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        try
        {
            _logger.LogInformation("Executing AI turn for session {SessionId}", gameSession.Id);

            var success = await _aiService.MakeAiTurnAsync(gameSession, cancellationToken);

            if (success)
            {
                _logger.LogInformation("AI turn executed successfully for session {SessionId}", gameSession.Id);
            }
            else
            {
                _logger.LogWarning("AI turn execution failed for session {SessionId}", gameSession.Id);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing AI turn for session {SessionId}: {Message}", gameSession.Id, ex.Message);
            return false;
        }
    }
}

