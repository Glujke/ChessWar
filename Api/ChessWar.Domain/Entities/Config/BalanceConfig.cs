namespace ChessWar.Domain.Entities.Config;

public sealed class BalanceConfig
{
    public required GlobalsSection Globals { get; init; }
    public required PlayerManaSection PlayerMana { get; init; }
    public required Dictionary<string, PieceStats> Pieces { get; init; }
    public required Dictionary<string, AbilitySpecModel> Abilities { get; init; }
    public required EvolutionSection Evolution { get; init; }
    public required AiSection Ai { get; init; }
    public required KillRewardsSection KillRewards { get; init; }
}



