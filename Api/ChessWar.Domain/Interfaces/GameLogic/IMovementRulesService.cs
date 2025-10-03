using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Сервис для определения правил движения шахматных фигур
/// </summary>
public interface IMovementRulesService
{
    /// <summary>
    /// Проверяет, может ли фигура переместиться с текущей позиции на целевую
    /// </summary>
    /// <param name="piece">Фигура для перемещения</param>
    /// <param name="targetPosition">Целевая позиция</param>
    /// <param name="boardPieces">Все фигуры на доске для проверки блокировок</param>
    /// <returns>True, если движение возможно</returns>
    bool CanMoveTo(Piece piece, Position targetPosition, IReadOnlyList<Piece> boardPieces);

    /// <summary>
    /// Получает все возможные ходы для фигуры
    /// </summary>
    /// <param name="piece">Фигура</param>
    /// <param name="boardPieces">Все фигуры на доске</param>
    /// <returns>Список возможных позиций для хода</returns>
    IReadOnlyList<Position> GetPossibleMoves(Piece piece, IReadOnlyList<Piece> boardPieces);

    /// <summary>
    /// Получает доступные ходы для фигуры (алиас для совместимости)
    /// </summary>
    List<Position> GetAvailableMoves(Piece piece, List<Piece> allPieces);

    /// <summary>
    /// Проверяет, находится ли позиция в пределах доски 8x8
    /// </summary>
    /// <param name="position">Позиция для проверки</param>
    /// <returns>True, если позиция валидна</returns>
    bool IsValidPosition(Position position);
}
