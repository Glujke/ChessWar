namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Exception thrown when target is out of range for the action
/// </summary>
public class OutOfRangeException : GameException
{
    public int MaxRange { get; }
    public int ActualDistance { get; }
    public Guid PieceId { get; }

    public OutOfRangeException(int maxRange, int actualDistance, Guid pieceId)
        : base($"Target is out of range. Max range: {maxRange}, Actual distance: {actualDistance}, PieceId: {pieceId}")
    {
        if (maxRange <= 0)
            throw new ArgumentException("Max range must be positive", nameof(maxRange));
        if (actualDistance < 0)
            throw new ArgumentException("Actual distance cannot be negative", nameof(actualDistance));

        MaxRange = maxRange;
        ActualDistance = actualDistance;
        PieceId = pieceId;
    }
}
