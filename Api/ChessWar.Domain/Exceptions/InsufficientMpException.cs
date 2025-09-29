namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Exception thrown when a piece doesn't have enough MP to perform an action
/// </summary>
public class InsufficientMpException : GameException
{
    public int RequiredMp { get; }
    public int AvailableMp { get; }
    public Guid PieceId { get; }

    public InsufficientMpException(int requiredMp, int availableMp, Guid pieceId)
        : base($"Insufficient MP to perform action. Required: {requiredMp}, Available: {availableMp}, PieceId: {pieceId}")
    {
        if (requiredMp <= 0)
            throw new ArgumentException("Required MP must be positive", nameof(requiredMp));
        if (availableMp < 0)
            throw new ArgumentException("Available MP cannot be negative", nameof(availableMp));

        RequiredMp = requiredMp;
        AvailableMp = availableMp;
        PieceId = pieceId;
    }
}
