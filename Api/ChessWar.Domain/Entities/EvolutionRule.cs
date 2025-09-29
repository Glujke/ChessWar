namespace ChessWar.Domain.Entities;

public class EvolutionRule
{
    public int Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
