namespace ChessWar.Application.DTOs;

public class GameBoardDto
{
    public List<PieceDto> Pieces { get; set; } = new();
    public int Size { get; set; } = 8;
}
