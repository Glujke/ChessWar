using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.TurnManagement;

/// <summary>
/// Реализация сервиса для записи действий в ход
/// </summary>
public class TurnActionRecorder : ITurnActionRecorder
{
    private readonly Turn _turn;

    public TurnActionRecorder(Turn turn)
    {
        _turn = turn ?? throw new ArgumentNullException(nameof(turn));
    }

    /// <summary>
    /// Записывает действие в ход
    /// </summary>
    public void RecordAction(string actionType, string pieceId, Position targetPosition, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(actionType))
            throw new ArgumentException("Action type cannot be null or empty", nameof(actionType));
        
        if (string.IsNullOrWhiteSpace(pieceId))
            throw new ArgumentException("Piece ID cannot be null or empty", nameof(pieceId));
        
        if (targetPosition == null)
            throw new ArgumentNullException(nameof(targetPosition));

        var action = new TurnAction(actionType, pieceId, targetPosition, description);
        _turn.AddAction(action);
    }
}
