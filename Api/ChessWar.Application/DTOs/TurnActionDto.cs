namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для действия в ходе
/// </summary>
public class TurnActionDto
{
    public string ActionType { get; set; } = string.Empty;
    public string PieceId { get; set; } = string.Empty;
    public PositionDto? TargetPosition { get; set; }
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
}