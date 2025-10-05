using ChessWar.Domain.Enums;

namespace ChessWar.Application.DTOs;

/// <summary>
/// DTO для фигуры
/// </summary>
public class PieceDto
{
    public int Id { get; set; }
    public PieceType Type { get; set; }
    public Team Team { get; set; }
    public PositionDto Position { get; set; } = null!;
    public int HP { get; set; }
    public int ATK { get; set; }
    public int Range { get; set; }
    public int Movement { get; set; }
    public int XP { get; set; }
    public int XPToEvolve { get; set; }
    public bool IsAlive { get; set; }
    public bool IsFirstMove { get; set; }
    public Dictionary<string, int> AbilityCooldowns { get; set; } = new();
    public int ShieldHp { get; set; }
    public int NeighborCount { get; set; }
}
