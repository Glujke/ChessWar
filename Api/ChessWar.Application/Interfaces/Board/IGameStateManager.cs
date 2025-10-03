using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Интерфейс для менеджера состояния игры
/// </summary>
public interface IGameStateManager
{
    /// <summary>
    /// Проверяет и обрабатывает завершение игры
    /// </summary>
    Task<GameResult?> CheckAndHandleGameCompletionAsync(GameSession gameSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Сохраняет состояние игры
    /// </summary>
    Task SaveGameStateAsync(GameSession gameSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, завершена ли игра
    /// </summary>
    Task<bool> IsGameCompletedAsync(GameSession gameSession, CancellationToken cancellationToken = default);
}

