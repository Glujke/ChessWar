namespace ChessWar.Domain.Entities;

public class BalanceVersion
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    
    public ICollection<PieceDefinition> Pieces { get; set; } = new List<PieceDefinition>();
    public ICollection<EvolutionRule> EvolutionRules { get; set; } = new List<EvolutionRule>();
    public GlobalRules? Globals { get; set; }
}
