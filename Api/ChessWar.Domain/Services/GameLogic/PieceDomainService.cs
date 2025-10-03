using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Services.GameLogic;

/// <summary>
/// Домашний сервис домена для базовых операций с фигурами (урон, лечение, перемещение, кулдауны).
/// </summary>
public class PieceDomainService : IPieceDomainService
{
    /// <summary>
    /// Применяет урон к фигуре и не допускает значения ниже нуля.
    /// </summary>
    public void TakeDamage(Piece piece, int damage)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (damage < 0) throw new ArgumentException("Damage cannot be negative", nameof(damage));

        piece.HP = Math.Max(0, piece.HP - damage);
    }

    /// <summary>
    /// Перемещает фигуру на указанную позицию и снимает флаг первого хода.
    /// </summary>
    public void MoveTo(Piece piece, Position newPosition)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (newPosition == null) throw new ArgumentNullException(nameof(newPosition));

        piece.Position = newPosition;
        piece.IsFirstMove = false;
    }

    /// <summary>
    /// Восстанавливает здоровье фигуры, не превышая максимальное значение.
    /// </summary>
    public void Heal(Piece piece, int amount)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (amount < 0) throw new ArgumentException("Heal amount cannot be negative", nameof(amount));

        var maxHP = GetMaxHP(piece.Type);
        piece.HP = Math.Min(maxHP, piece.HP + amount);
    }

    /// <summary>
    /// Возвращает максимальное количество здоровья для типа фигуры.
    /// </summary>
    public int GetMaxHP(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 10,
            PieceType.Knight => 20,
            PieceType.Bishop => 18,
            PieceType.Rook => 25,
            PieceType.Queen => 30,
            PieceType.King => 50,
            _ => 0
        };
    }

    /// <summary>
    /// Добавляет опыт фигуре на указанную величину.
    /// </summary>
    public void AddXP(Piece piece, int amount)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (amount < 0) throw new ArgumentException("XP amount cannot be negative", nameof(amount));

        piece.XP += amount;
    }

    /// <summary>
    /// Синоним для AddXP для совместимости.
    /// </summary>
    public void AddExperience(Piece piece, int amount)
    {
        AddXP(piece, amount);
    }

    /// <summary>
    /// Возвращает true, если здоровье фигуры равно нулю или ниже.
    /// </summary>
    public bool IsDead(Piece piece)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        return piece.HP <= 0;
    }

    /// <summary>
    /// Уменьшает кулдауны способностей фигуры на один тик.
    /// </summary>
    public void ReduceCooldowns(Piece piece)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        TickCooldowns(piece);
    }

    /// <summary>
    /// Обновляет кулдауны способностей и снимает эффекты по завершении кулдауна.
    /// </summary>
    public void TickCooldowns(Piece piece)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        var keys = piece.AbilityCooldowns.Keys.ToList();
        foreach (var key in keys)
        {
            if (piece.AbilityCooldowns[key] > 0)
            {
                piece.AbilityCooldowns[key]--;

                if (key == "__FortressBuff" && piece.AbilityCooldowns[key] == 0)
                {
                    var maxHP = GetMaxHP(piece.Type);
                    if (piece.HP > maxHP)
                    {
                        piece.HP = maxHP;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Проверяет, доступна ли способность (кулдаун равен нулю).
    /// </summary>
    public bool CanUseAbility(Piece piece, string abilityName)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (string.IsNullOrWhiteSpace(abilityName)) throw new ArgumentException("Ability name cannot be null or empty", nameof(abilityName));

        return piece.AbilityCooldowns.GetValueOrDefault(abilityName, 0) == 0;
    }

    /// <summary>
    /// Устанавливает кулдаун (в ходах) для способности фигуры.
    /// </summary>
    public void SetAbilityCooldown(Piece piece, string abilityName, int cooldown)
    {
        if (piece == null) throw new ArgumentNullException(nameof(piece));
        if (string.IsNullOrWhiteSpace(abilityName)) throw new ArgumentException("Ability name cannot be null or empty", nameof(abilityName));
        if (cooldown < 0) throw new ArgumentException("Cooldown cannot be negative", nameof(cooldown));

        piece.AbilityCooldowns[abilityName] = cooldown;
    }
}
