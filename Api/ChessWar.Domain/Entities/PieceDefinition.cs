namespace ChessWar.Domain.Entities;

public class PieceDefinition
{
    public int Id { get; set; }
    public string PieceId { get; set; } = string.Empty;
    public int HP { get; set; }
    public int ATK { get; set; }
    public int Range { get; set; }
    public int Movement { get; set; }
    public int Energy { get; set; }
    public int ExpToEvolve { get; set; }
}
