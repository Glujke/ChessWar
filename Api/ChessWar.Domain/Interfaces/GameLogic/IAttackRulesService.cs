using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Сервис для проверки правил атак в игре Chess War
/// </summary>
public interface IAttackRulesService
{
    /// <summary>
    /// Проверяет, может ли фигура атаковать цель в указанной позиции
    /// </summary>
    /// <param name="attacker">Атакующая фигура</param>
    /// <param name="targetPosition">Позиция цели</param>
    /// <param name="boardPieces">Все фигуры на доске для проверки препятствий</param>
    /// <returns>True, если атака возможна</returns>
    bool CanAttack(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces);

    /// <summary>
    /// Получает все позиции, которые может атаковать фигура
    /// </summary>
    /// <param name="attacker">Атакующая фигура</param>
    /// <param name="boardPieces">Все фигуры на доске</param>
    /// <returns>Список позиций, которые может атаковать фигура</returns>
    IEnumerable<Position> GetAttackablePositions(Piece attacker, IEnumerable<Piece> boardPieces);

    /// <summary>
    /// Получает доступные атаки для фигуры (алиас для совместимости)
    /// </summary>
    List<Position> GetAvailableAttacks(Piece attacker, List<Piece> allPieces);

    /// <summary>
    /// Проверяет, является ли цель союзником или врагом
    /// </summary>
    /// <param name="attacker">Атакующая фигура</param>
    /// <param name="target">Целевая фигура</param>
    /// <returns>True, если цель является врагом (можно атаковать)</returns>
    bool IsEnemy(Piece attacker, Piece target);

    /// <summary>
    /// Вычисляет расстояние Чебышёва между двумя позициями
    /// </summary>
    /// <param name="from">Начальная позиция</param>
    /// <param name="to">Конечная позиция</param>
    /// <returns>Расстояние Чебышёва</returns>
    int CalculateChebyshevDistance(Position from, Position to);

    /// <summary>
    /// Проверяет, есть ли препятствия на пути атаки
    /// </summary>
    /// <param name="attacker">Атакующая фигура</param>
    /// <param name="targetPosition">Позиция цели</param>
    /// <param name="boardPieces">Все фигуры на доске</param>
    /// <returns>True, если путь свободен</returns>
    bool IsPathClear(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces);

    /// <summary>
    /// Проверяет, есть ли валидная цель для атаки
    /// </summary>
    /// <param name="attacker">Атакующая фигура</param>
    /// <param name="targetPosition">Позиция цели</param>
    /// <param name="boardPieces">Все фигуры на доске</param>
    /// <returns>True, если есть живая вражеская фигура в позиции</returns>
    bool HasValidTarget(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces);

    /// <summary>
    /// Проверяет, может ли фигура атаковать позицию по радиусу и правилам (без проверки цели)
    /// </summary>
    /// <param name="attacker">Атакующая фигура</param>
    /// <param name="targetPosition">Позиция цели</param>
    /// <param name="boardPieces">Все фигуры на доске</param>
    /// <returns>True, если позиция в радиусе атаки и путь свободен</returns>
    bool IsWithinAttackRange(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces);
}
