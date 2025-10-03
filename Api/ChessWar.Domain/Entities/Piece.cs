using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Представляет шахматную фигуру в игре
/// </summary>
public class Piece
{
    public int Id { get; set; }
    public PieceType Type { get; set; }
    public Team Team { get; set; }
    public Position Position { get; set; } = new(0, 0);
    public Participant? Owner { get; set; }

    public int HP { get; set; }
    public int ATK { get; set; }
    public int Attack => ATK;
    public int Range { get; set; }
    public int Movement { get; set; }
    public int XP { get; set; }
    public int XPToEvolve { get; set; }

    public bool CanEvolve => XP >= XPToEvolve;
    public bool IsAlive => HP > 0;
    public bool IsFirstMove { get; set; } = true;

    public Dictionary<string, int> AbilityCooldowns { get; set; } = new();

    /// <summary>
    /// Начисляет опыт фигуре
    /// </summary>
    public void GainExperience(int experience)
    {
        XP += experience;
    }

    public Piece() { }

    /// <summary>
    /// Создает новую фигуру
    /// </summary>
    public Piece(PieceType type, Team team, Position position)
    {
        Type = type;
        Team = team;
        Position = position;
    }

    /// <summary>
    /// Создает новую фигуру с указанным ID и владельцем
    /// </summary>
    public Piece(string id, PieceType type, Team team, Position position, Participant owner)
    {
        Id = int.TryParse(id, out var parsedId) ? parsedId : 0;
        Type = type;
        Team = team;
        Position = position;
        Owner = owner;
    }
}
