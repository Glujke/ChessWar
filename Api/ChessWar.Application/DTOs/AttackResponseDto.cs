namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для ответа о возможности атаки
/// </summary>
public class AttackResponseDto
{
    /// <summary>
    /// Можно ли атаковать
    /// </summary>
    public bool CanAttack { get; set; }

    /// <summary>
    /// Причина, почему нельзя атаковать (если CanAttack = false)
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
}
