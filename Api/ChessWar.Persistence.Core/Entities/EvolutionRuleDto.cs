namespace ChessWar.Persistence.Core.Entities;

public class EvolutionRuleDto
{
    public int Id { get; set; }
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}
