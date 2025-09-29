using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Interfaces.Pieces;

/// <summary>
/// Сервис приложения для управления атаками в игре Chess War
/// </summary>
public interface IAttackApplicationService
{
    /// <summary>
    /// Проверяет возможность атаки и возвращает детальную информацию
    /// </summary>
    /// <param name="attackerId">ID атакующей фигуры</param>
    /// <param name="targetPosition">Позиция цели</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат проверки атаки</returns>
    Task<AttackApplicationResult> CheckAttackAsync(int attackerId, Position targetPosition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает все позиции, которые может атаковать фигура
    /// </summary>
    /// <param name="attackerId">ID атакующей фигуры</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Список атакуемых позиций</returns>
    Task<IEnumerable<Position>> GetAttackablePositionsAsync(int attackerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, является ли цель врагом
    /// </summary>
    /// <param name="attackerId">ID атакующей фигуры</param>
    /// <param name="targetId">ID целевой фигуры</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>True, если цель является врагом</returns>
    Task<bool> IsEnemyAsync(int attackerId, int targetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Вычисляет расстояние Чебышёва между двумя позициями
    /// </summary>
    /// <param name="from">Начальная позиция</param>
    /// <param name="to">Конечная позиция</param>
    /// <returns>Расстояние Чебышёва</returns>
    int CalculateChebyshevDistance(Position from, Position to);
}

