namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для диспетчера уведомлений
/// </summary>
public interface INotificationDispatcher
{
    void DispatchTurnEnded(Guid sessionId, string participantType, int turnNumber);
    void DispatchTurnStarted(Guid sessionId, string participantType, int turnNumber);
    void DispatchAITurnInProgress(Guid sessionId);
    void DispatchAITurnCompleted(Guid sessionId);
    void DispatchAiMove(Guid sessionId, object moveData);
    void DispatchGameEnded(Guid sessionId, string result, string message);
    void DispatchPieceEvolved(Guid sessionId, string pieceId, string newType, int x, int y);
    void DispatchError(Guid sessionId, string error);
}

