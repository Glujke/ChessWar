namespace ChessWar.Application.Interfaces.GameManagement;

/// <summary>
/// Сервис для отправки уведомлений клиентам через SignalR
/// </summary>
public interface IGameNotificationService
{
    /// <summary>
    /// Уведомляет о ходе ИИ
    /// </summary>
    Task NotifyAiMoveAsync(Guid sessionId, object moveData, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет о завершении игры
    /// </summary>
    Task NotifyGameEndedAsync(Guid sessionId, string result, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет об ошибке
    /// </summary>
    Task NotifyErrorAsync(Guid sessionId, string error, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет о переходе стадии в туториале
    /// </summary>
    Task NotifyTutorialAdvancedAsync(Guid tutorialId, string stage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет об эволюции фигуры
    /// </summary>
    Task NotifyPieceEvolvedAsync(Guid sessionId, string pieceId, string newType, int x, int y, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет о начале хода ИИ
    /// </summary>
    Task NotifyAITurnInProgressAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет о завершении хода ИИ
    /// </summary>
    Task NotifyAITurnCompletedAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет о начале хода
    /// </summary>
    Task NotifyTurnStartedAsync(Guid sessionId, string participantType, int turnNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Уведомляет о завершении хода
    /// </summary>
    Task NotifyTurnEndedAsync(Guid sessionId, string participantType, int turnNumber, CancellationToken cancellationToken = default);
}
