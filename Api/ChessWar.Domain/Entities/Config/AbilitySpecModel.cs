namespace ChessWar.Domain.Entities.Config;

public sealed class AbilitySpecModel
{
    public int MpCost { get; init; }
    public int Cooldown { get; init; }
    public int Range { get; init; }
    public bool IsAoe { get; init; }
    public int Damage { get; init; }
    public int Heal { get; init; }
    public int Hits { get; init; }
    public int DamagePerHit { get; init; }
    public int TempHpMultiplier { get; init; }
    public int DurationTurns { get; init; }
    public bool GrantsExtraTurn { get; init; }
    public bool DiagonalOnly { get; init; }
    public int RestoreHpPercent { get; init; }
}



