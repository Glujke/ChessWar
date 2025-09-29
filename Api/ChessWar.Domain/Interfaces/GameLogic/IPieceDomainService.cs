using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Интерфейс для Domain Service работы с фигурами
/// </summary>
public interface IPieceDomainService
{
    /// <summary>
    /// Наносит урон фигуре
    /// </summary>
    void TakeDamage(Piece piece, int damage);

    /// <summary>
    /// Перемещает фигуру на новую позицию
    /// </summary>
    void MoveTo(Piece piece, Position newPosition);

    /// <summary>
    /// Лечит фигуру
    /// </summary>
    void Heal(Piece piece, int amount);

    /// <summary>
    /// Получает максимальное HP для типа фигуры
    /// </summary>
    int GetMaxHP(PieceType type);

    /// <summary>
    /// Добавляет опыт фигуре
    /// </summary>
    void AddXP(Piece piece, int amount);

    /// <summary>
    /// Добавляет опыт фигуре (алиас для совместимости)
    /// </summary>
    void AddExperience(Piece piece, int amount);

    /// <summary>
    /// Проверяет, мертва ли фигура
    /// </summary>
    bool IsDead(Piece piece);

    /// <summary>
    /// Уменьшает кулдауны способностей
    /// </summary>
    void ReduceCooldowns(Piece piece);

    /// <summary>
    /// Обновляет кулдауны способностей
    /// </summary>
    void TickCooldowns(Piece piece);

    /// <summary>
    /// Проверяет, может ли фигура использовать способность
    /// </summary>
    bool CanUseAbility(Piece piece, string abilityName);

    /// <summary>
    /// Устанавливает кулдаун для способности
    /// </summary>
    void SetAbilityCooldown(Piece piece, string abilityName, int cooldown);
}
