using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.AI;

public interface IAIService
{
    /// <summary>
    /// Выполняет ход за активного участника (ИИ) и возвращает true, если действие выполнено.
    /// </summary>
    bool MakeAiTurn(GameSession session);
    
    /// <summary>
    /// Выполняет ход за активного участника (ИИ) асинхронно и возвращает true, если действие выполнено.
    /// </summary>
    Task<bool> MakeAiTurnAsync(GameSession session, CancellationToken cancellationToken = default);
}


