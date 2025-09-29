namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для ответа со списком атакуемых позиций
/// </summary>
public class AttackablePositionsResponseDto
{
    /// <summary>
    /// ID атакующей фигуры
    /// </summary>
    public int AttackerId { get; set; }

    /// <summary>
    /// Список позиций, которые можно атаковать
    /// </summary>
    public List<PositionDto> AttackablePositions { get; set; } = new();

    /// <summary>
    /// Общее количество атакуемых позиций
    /// </summary>
    public int TotalCount { get; set; }
}
