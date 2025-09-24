namespace ChessWar.Persistence.Core.Entities;

public class BalanceVersionDto
{
    public Guid Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }
    
    public ICollection<PieceDefinitionDto> Pieces { get; set; } = new List<PieceDefinitionDto>();
    public ICollection<EvolutionRuleDto> EvolutionRules { get; set; } = new List<EvolutionRuleDto>();
    public GlobalRulesDto? Globals { get; set; }
}
