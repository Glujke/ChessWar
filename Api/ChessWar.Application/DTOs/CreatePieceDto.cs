namespace ChessWar.Application.DTOs;

public class CreatePieceDto
{
    public string Type { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
}
