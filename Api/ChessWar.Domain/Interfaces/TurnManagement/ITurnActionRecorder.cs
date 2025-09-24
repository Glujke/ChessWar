using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.TurnManagement;

/// <summary>
/// Интерфейс для записи действий в ход
/// </summary>
public interface ITurnActionRecorder
{
    /// <summary>
    /// Записывает действие в ход
    /// </summary>
    /// <param name="actionType">Тип действия (Move, Attack, Ability)</param>
    /// <param name="pieceId">ID фигуры, выполняющей действие</param>
    /// <param name="targetPosition">Целевая позиция</param>
    /// <param name="description">Дополнительное описание (например, название способности)</param>
    void RecordAction(string actionType, string pieceId, Position targetPosition, string? description = null);
}
