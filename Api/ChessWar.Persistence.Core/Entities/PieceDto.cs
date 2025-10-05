using ChessWar.Domain.Enums;

namespace ChessWar.Persistence.Core.Entities;

public class PieceDto
{
    public int Id { get; set; }
    public PieceType Type { get; set; }
    public Team Team { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    public int HP { get; set; }
    public int ATK { get; set; }
    public int Range { get; set; }
    public int Movement { get; set; }
    public int XP { get; set; }
    public int XPToEvolve { get; set; }

    public bool IsFirstMove { get; set; } = true;

    public string AbilityCooldownsJson { get; set; } = "{}";
    public int ShieldHp { get; set; }
    public int NeighborCount { get; set; }
}
