namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для выполнения действия в ходе
/// </summary>
public class ExecuteActionDto
{
    public string Type { get; set; } = string.Empty;
    public string PieceId { get; set; } = string.Empty;
    public PositionDto? TargetPosition { get; set; }
    public string? Description { get; set; }
}