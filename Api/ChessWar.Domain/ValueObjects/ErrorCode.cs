namespace ChessWar.Domain.ValueObjects;

public record ErrorCode(string Code, string Description)
{
    public override string ToString() => Code;
}
