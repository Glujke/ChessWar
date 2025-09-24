using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Базовый интерфейс для сценариев
/// </summary>
public interface IScenario
{
    /// <summary>
    /// Тип сценария
    /// </summary>
    ScenarioType Type { get; }
    
    /// <summary>
    /// Название сценария
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Описание сценария
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Завершен ли сценарий
    /// </summary>
    bool IsCompleted { get; }
    
    /// <summary>
    /// Прогресс выполнения (0-100)
    /// </summary>
    int Progress { get; }
    
    /// <summary>
    /// Обновляет прогресс сценария
    /// </summary>
    void UpdateProgress(int progress);
}

