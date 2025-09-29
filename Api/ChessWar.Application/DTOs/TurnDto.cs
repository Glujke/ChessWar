namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для хода
/// </summary>
public class TurnDto
{
    public int Number { get; set; }
    public PlayerDto ActiveParticipant { get; set; } = null!;
    public PieceDto? SelectedPiece { get; set; }
    public List<TurnActionDto> Actions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
