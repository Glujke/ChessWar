using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.AI;

/// <summary>
/// Сервис выполнения ходов ИИ
/// </summary>
public interface IAITurnService
{
    /// <summary>
    /// Выполняет ход ИИ
    /// </summary>
    Task<bool> MakeAiTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);
}
