using ChessWar.Application.Interfaces.GameManagement; using ChessWar.Application.Interfaces.Configuration;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.GameManagement;

/// <summary>
/// Реализация сервиса уведомлений в Application слое
/// </summary>
public class GameNotificationService : IGameNotificationService
{
    private readonly IGameHubClient _hubClient;
    private readonly ILogger<GameNotificationService> _logger;

    public GameNotificationService(
        IGameHubClient hubClient,
        ILogger<GameNotificationService> logger)
    {
        _hubClient = hubClient ?? throw new ArgumentNullException(nameof(hubClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task NotifyAiMoveAsync(Guid sessionId, object moveData, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notified clients about AI move in session {SessionId}", sessionId);
        await _hubClient.SendToGroupAsync(sessionId.ToString(), "AiMoved", moveData, cancellationToken);
    }

    public async Task NotifyGameEndedAsync(Guid sessionId, string result, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notified clients about game end in session {SessionId} with result {Result}", sessionId, result);
        await _hubClient.SendToGroupAsync(sessionId.ToString(), "GameEnded", new { SessionId = sessionId, Result = result, Message = message, Timestamp = DateTime.UtcNow }, cancellationToken);
    }

    public async Task NotifyErrorAsync(Guid sessionId, string error, CancellationToken cancellationToken = default)
    {
        _logger.LogError("Notified clients about error in session {SessionId}: {Error}", sessionId, error);
        await _hubClient.SendToGroupAsync(sessionId.ToString(), "Error", new { SessionId = sessionId, Error = error, Timestamp = DateTime.UtcNow }, cancellationToken);
    }

    public async Task NotifyTutorialAdvancedAsync(Guid tutorialId, string stage, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Notified clients about tutorial advancement in session {TutorialId} to stage {Stage}", tutorialId, stage);
        await _hubClient.SendToGroupAsync(tutorialId.ToString(), "TutorialAdvanced", new { TutorialId = tutorialId, Stage = stage, Timestamp = DateTime.UtcNow }, cancellationToken);
    }
}
