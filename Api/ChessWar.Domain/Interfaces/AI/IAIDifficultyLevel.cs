using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Уровни сложности ИИ
/// </summary>
public enum AIDifficultyLevel
{
    /// <summary>
    /// Легкий - случайные действия с базовыми приоритетами
    /// </summary>
    Easy = 1,
    
    /// <summary>
    /// Средний - вероятностное планирование на 2-3 хода
    /// </summary>
    Medium = 2,
    
    /// <summary>
    /// Сложный - полное вероятностное планирование с Monte Carlo
    /// </summary>
    Hard = 3
}

/// <summary>
/// Интерфейс для определения уровня сложности ИИ
/// </summary>
public interface IAIDifficultyLevel
{
    /// <summary>
    /// Получить уровень сложности для игрока
    /// </summary>
    AIDifficultyLevel GetDifficultyLevel(Player player);
    
    /// <summary>
    /// Получить температуру для softmax (τ)
    /// </summary>
    double GetTemperature(AIDifficultyLevel level);
    
    /// <summary>
    /// Получить глубину планирования
    /// </summary>
    int GetPlanningDepth(AIDifficultyLevel level);
    
    /// <summary>
    /// Получить коэффициент дисконтирования (γ)
    /// </summary>
    double GetDiscountFactor(AIDifficultyLevel level);
}
