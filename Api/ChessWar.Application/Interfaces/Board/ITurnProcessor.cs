using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для единого процессора ходов
/// </summary>
public interface ITurnProcessor
{
    /// <summary>
    /// Обрабатывает полную фазу хода (игрок + AI + tick CD)
    /// </summary>
    Task ProcessTurnPhaseAsync(GameSession gameSession, CancellationToken cancellationToken = default);
}

