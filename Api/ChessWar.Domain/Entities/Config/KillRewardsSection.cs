namespace ChessWar.Domain.Entities.Config;

public sealed class KillRewardsSection
{
    public int Pawn { get; init; }
    public int Knight { get; init; }
    public int Bishop { get; init; }
    public int Rook { get; init; }
    public int Queen { get; init; }
    public int King { get; init; }
}
