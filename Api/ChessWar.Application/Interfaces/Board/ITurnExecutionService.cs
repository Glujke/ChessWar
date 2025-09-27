using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Сервис выполнения ходов - объединяет все операции с ходами
/// </summary>
public interface ITurnExecutionService
{
    /// <summary>
    /// Выполняет действие в ходе
    /// </summary>
    Task<bool> ExecuteActionAsync(GameSession gameSession, ExecuteActionDto dto);

    /// <summary>
    /// Завершает текущий ход
    /// </summary>
    Task EndTurnAsync(GameSession gameSession);

    /// <summary>
    /// Выполняет ход ИИ
    /// </summary>
    Task<bool> MakeAiTurnAsync(GameSession gameSession);

    /// <summary>
    /// Выполняет перемещение фигуры
    /// </summary>
    Task<bool> ExecuteMoveAsync(GameSession gameSession, int pieceId, Position targetPosition);

    /// <summary>
    /// Получает доступные действия для фигуры
    /// </summary>
    Task<List<Position>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, string? abilityName = null);
}
