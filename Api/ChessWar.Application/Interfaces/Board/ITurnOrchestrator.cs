using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Координатор ходов - управляет завершением ходов и связанными операциями
/// </summary>
public interface ITurnOrchestrator
{
    /// <summary>
    /// Завершает ход и выполняет все связанные операции (ИИ, проверка победы, уведомления)
    /// </summary>
    Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);
}
