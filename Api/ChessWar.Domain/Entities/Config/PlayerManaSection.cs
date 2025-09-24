namespace ChessWar.Domain.Entities.Config;

public sealed class PlayerManaSection
{
    public int InitialMana { get; init; } = 10;
    public int MaxMana { get; init; } = 50;
    public int ManaRegenPerTurn { get; init; } = 10;
    public bool MandatoryAction { get; init; } = true;
    public Dictionary<string, int> MovementCosts { get; init; } = new()
    {
        ["Pawn"] = 1,
        ["Knight"] = 2,
        ["Bishop"] = 3,
        ["Rook"] = 3,
        ["Queen"] = 4,
        ["King"] = 4
    };
    public int AttackCost { get; init; } = 1;
}


