namespace ChessWar.Domain.ValueObjects;

/// <summary>
/// Действие в ходе
/// </summary>
public class TurnAction
{
    public string ActionType { get; private set; }
    public string PieceId { get; private set; }
    public Position? TargetPosition { get; private set; }
    public string? Description { get; private set; }
    public DateTime Timestamp { get; private set; }

    public TurnAction(string actionType, string pieceId, Position? targetPosition = null, string? description = null)
    {
        ActionType = actionType ?? throw new ArgumentNullException(nameof(actionType));
        PieceId = pieceId ?? throw new ArgumentNullException(nameof(pieceId));
        TargetPosition = targetPosition;
        Description = description;
        Timestamp = DateTime.UtcNow;
    }
}
