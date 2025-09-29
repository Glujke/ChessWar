namespace ChessWar.Domain.Entities.Config;

public sealed class GlobalsSection
{
    public int MpRegenPerTurn { get; init; }
    public string CooldownTickPhase { get; init; } = "EndTurn";
}



