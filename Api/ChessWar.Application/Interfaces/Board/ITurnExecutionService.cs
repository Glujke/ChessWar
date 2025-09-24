using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Сервис выполнения действий в ходе
/// </summary>
public interface ITurnExecutionService
{
    /// <summary>
    /// Выполняет действие в ходе
    /// </summary>
    Task<bool> ExecuteActionAsync(GameSession gameSession, ExecuteActionDto dto, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Завершает текущий ход
    /// </summary>
    Task EndTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Выполняет ход ИИ
    /// </summary>
    Task<bool> MakeAiTurnAsync(GameSession gameSession, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает доступные действия для фигуры
    /// </summary>
    Task<List<PositionDto>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, string? abilityName = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Выполняет движение фигуры
    /// </summary>
    Task<bool> ExecuteMoveAsync(GameSession gameSession, int pieceId, Position targetPosition, CancellationToken cancellationToken = default);
}
