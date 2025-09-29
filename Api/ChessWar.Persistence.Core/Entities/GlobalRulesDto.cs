namespace ChessWar.Persistence.Core.Entities;

public class GlobalRulesDto
{
    public int Id { get; set; }
    public int MpRegenPerTurn { get; set; }
    public string CooldownTickPhase { get; set; } = string.Empty;
}
