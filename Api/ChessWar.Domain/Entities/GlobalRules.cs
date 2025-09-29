namespace ChessWar.Domain.Entities;

public class GlobalRules
{
    public int Id { get; set; }
    public int MpRegenPerTurn { get; set; }
    public string CooldownTickPhase { get; set; } = string.Empty;
}
