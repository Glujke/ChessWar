namespace ChessWar.Domain.Exceptions;

/// <summary>
/// Исключение при заблокированной линии обзора для действия.
/// </summary>
public class LineOfSightBlockedException : GameException
{
    public Guid PieceId { get; }
    public Guid BlockingPieceId { get; }

    public LineOfSightBlockedException(Guid pieceId, Guid blockingPieceId)
        : base($"Line of sight is blocked by piece {blockingPieceId}. Attacking piece: {pieceId}")
    {
        if (pieceId == Guid.Empty)
            throw new ArgumentException("Piece ID cannot be empty", nameof(pieceId));
        if (blockingPieceId == Guid.Empty)
            throw new ArgumentException("Blocking piece ID cannot be empty", nameof(blockingPieceId));

        PieceId = pieceId;
        BlockingPieceId = blockingPieceId;
    }
}
