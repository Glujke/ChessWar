namespace ChessWar.Application.DTOs;

public class AbilityRequestDto
{
    public string PieceId { get; set; } = string.Empty;
    public string AbilityName { get; set; } = string.Empty;
    public PositionDto Target { get; set; } = new PositionDto();
}


