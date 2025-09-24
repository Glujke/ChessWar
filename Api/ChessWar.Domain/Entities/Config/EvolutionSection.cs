namespace ChessWar.Domain.Entities.Config;

public sealed class EvolutionSection
{
    public required Dictionary<string, int> XpThresholds { get; init; }
    public required Dictionary<string, List<string>> Rules { get; init; }
    public Dictionary<string, bool>? ImmediateOnLastRank { get; init; }
}



