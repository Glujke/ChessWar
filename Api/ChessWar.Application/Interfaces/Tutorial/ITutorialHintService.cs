using ChessWar.Domain.Enums;

namespace ChessWar.Application.Interfaces.Tutorial;

/// <summary>
/// Интерфейс для сервиса подсказок обучения
/// </summary>
public interface ITutorialHintService
{
    /// <summary>
    /// Получает подсказки для конкретного этапа
    /// </summary>
    Task<List<string>> GetHintsForStageAsync(TutorialStage stage, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает контекстные подсказки на основе состояния игры
    /// </summary>
    Task<List<string>> GetContextualHintsAsync(Guid sessionId, string gameState, CancellationToken cancellationToken = default);
}
