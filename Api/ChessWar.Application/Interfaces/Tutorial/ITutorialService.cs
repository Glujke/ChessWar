using ChessWar.Domain.Interfaces.Tutorial;

namespace ChessWar.Application.Interfaces.Tutorial;

/// <summary>
/// Интерфейс для сервиса обучения
/// </summary>
public interface ITutorialService
{
    /// <summary>
    /// Запускает обучение для игрока
    /// </summary>
    Task<ITutorialMode> StartTutorialAsync(string playerId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Переходит к следующему этапу обучения
    /// </summary>
    Task<ITutorialMode> AdvanceToNextStageAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает прогресс обучения
    /// </summary>
    Task<int> GetTutorialProgressAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает подсказки для текущего этапа
    /// </summary>
    Task<List<string>> GetCurrentHintsAsync(Guid sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Завершает обучение
    /// </summary>
    Task<ITutorialMode> CompleteTutorialAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
