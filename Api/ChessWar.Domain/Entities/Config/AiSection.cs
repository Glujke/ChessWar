namespace ChessWar.Domain.Entities.Config;

public sealed class AiSection
{
    public int NearEvolutionXp { get; init; }
    public Dictionary<string, int>? LastRankEdgeY { get; init; }
    public KingAuraConfig? KingAura { get; init; }
}

public sealed class KingAuraConfig
{
    public int Radius { get; init; }
    public int AtkBonus { get; init; }
}



