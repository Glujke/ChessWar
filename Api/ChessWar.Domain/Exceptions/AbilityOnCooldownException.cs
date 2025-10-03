namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Исключение, возникающее при попытке использовать способность, находящуюся на кулдауне.
/// </summary>
public class AbilityOnCooldownException : GameException
{
    public string AbilityName { get; }
    public int RemainingCooldown { get; }
    public Guid PieceId { get; }

    public AbilityOnCooldownException(string abilityName, int remainingCooldown, Guid pieceId)
        : base($"Ability '{abilityName}' is on cooldown. Remaining: {remainingCooldown} turns, PieceId: {pieceId}")
    {
        if (string.IsNullOrWhiteSpace(abilityName))
            throw new ArgumentException("Ability name cannot be null or empty", nameof(abilityName));
        if (remainingCooldown < 0)
            throw new ArgumentException("Remaining cooldown cannot be negative", nameof(remainingCooldown));

        AbilityName = abilityName;
        RemainingCooldown = remainingCooldown;
        PieceId = pieceId;
    }
}
