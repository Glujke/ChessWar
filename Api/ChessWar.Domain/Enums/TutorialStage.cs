namespace ChessWar.Domain.Enums;

/// <summary>
/// Этапы обучения в Tutorial режиме
/// </summary>
public enum TutorialStage
{
    /// <summary>
    /// Начальный этап - изучение основ
    /// </summary>
    Introduction = 0,

    /// <summary>
    /// Первый бой с ИИ (легкая сложность)
    /// </summary>
    Battle1 = 1,

    /// <summary>
    /// Второй бой с ИИ (средняя сложность)
    /// </summary>
    Battle2 = 2,

    /// <summary>
    /// Финальный бой с боссом
    /// </summary>
    Boss = 3,

    /// <summary>
    /// Обучение завершено
    /// </summary>
    Completed = 4
}
