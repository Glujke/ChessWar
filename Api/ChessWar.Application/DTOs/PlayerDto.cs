namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для игрока
/// </summary>
public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PieceDto> Pieces { get; set; } = new();
    public int Victories { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MP { get; set; }
    public int MaxMP { get; set; }
}
