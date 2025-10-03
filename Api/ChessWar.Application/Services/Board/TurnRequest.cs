using ChessWar.Domain.Entities;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Запрос на обработку хода
/// </summary>
public class TurnRequest
{
    public Guid SessionId { get; set; }
    public GameSession GameSession { get; set; }
    public TurnRequestType Type { get; set; }
    public int Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public TurnRequest(Guid sessionId, GameSession gameSession, TurnRequestType type, int priority = 0)
    {
        SessionId = sessionId;
        GameSession = gameSession;
        Type = type;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;
        CancellationToken = CancellationToken.None;
    }
}

/// <summary>
/// Тип запроса на обработку хода
/// </summary>
public enum TurnRequestType
{
    PlayerTurn,
    AITurn,
    TutorialTurn
}

