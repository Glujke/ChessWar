using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces;

/// <summary>
/// Результат проверки атаки на уровне приложения
/// </summary>
public class AttackApplicationResult
{
    /// <summary>
    /// Можно ли атаковать
    /// </summary>
    public bool CanAttack { get; set; }

    /// <summary>
    /// Причина, почему нельзя атаковать
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Расстояние до цели
    /// </summary>
    public int Distance { get; set; }

    /// <summary>
    /// Максимальный радиус атаки фигуры
    /// </summary>
    public int MaxRange { get; set; }

    /// <summary>
    /// Атакующая фигура
    /// </summary>
    public Piece? Attacker { get; set; }
}
