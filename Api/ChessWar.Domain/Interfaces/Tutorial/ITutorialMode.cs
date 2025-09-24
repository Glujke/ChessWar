using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Interfaces.Tutorial;

/// <summary>
/// Интерфейс для сессии обучения
/// </summary>
public interface ITutorialMode : IGameModeBase
{
    /// <summary>
    /// Текущий этап обучения
    /// </summary>
    TutorialStage CurrentStage { get; }
    
    /// <summary>
    /// Прогресс прохождения (0-100)
    /// </summary>
    int Progress { get; }
    
    /// <summary>
    /// Показывать ли подсказки
    /// </summary>
    bool ShowHints { get; }
    
    /// <summary>
    /// Завершено ли обучение
    /// </summary>
    bool IsCompleted { get; }
    
    /// <summary>
    /// Переходит к следующему этапу
    /// </summary>
    void AdvanceToNextStage();
    
    /// <summary>
    /// Устанавливает активный сценарий
    /// </summary>
    void SetActiveScenario(IScenario scenario);
}
