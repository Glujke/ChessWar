using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Сервис завершения ходов
/// </summary>
public interface ITurnCompletionService
{
    /// <summary>
    /// Завершает текущий ход
    /// </summary>
    Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);
}
