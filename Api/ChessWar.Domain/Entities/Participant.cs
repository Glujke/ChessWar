using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Абстрактный участник игры - базовый класс для игроков и ИИ
/// </summary>
public abstract class Participant
{
    public Guid Id { get; protected set; }
    public string Name { get; protected set; }
    public List<Piece> Pieces { get; protected set; }
    public int Victories { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public int MP { get; protected set; }
    public int MaxMP { get; protected set; }

    protected Participant(string name)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Pieces = new List<Piece>();
        Victories = 0;
        CreatedAt = DateTime.UtcNow;
        MP = 0;
        MaxMP = 0;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Participant name cannot be empty", nameof(name));
        }
    }

    /// <summary>
    /// Добавляет победу участнику
    /// </summary>
    public void AddVictory()
    {
        Victories++;
    }

    /// <summary>
    /// Устанавливает ману участнику
    /// </summary>
    public void SetMana(int current, int max)
    {
        MP = Math.Max(0, current);
        MaxMP = Math.Max(MP, max);
    }

    /// <summary>
    /// Тратит ману
    /// </summary>
    public bool SpendMana(int amount)
    {
        if (MP < amount)
            return false;

        MP -= amount;
        return true;
    }

    /// <summary>
    /// Восстанавливает ману
    /// </summary>
    public void RestoreMana(int amount)
    {
        MP = Math.Min(MaxMP, MP + amount);
    }

    /// <summary>
    /// Проверяет, может ли потратить указанное количество маны
    /// </summary>
    public bool CanSpend(int amount)
    {
        if (amount < 0) return false;
        return MP >= amount;
    }

    /// <summary>
    /// Тратит ману
    /// </summary>
    public void Spend(int amount)
    {
        if (!CanSpend(amount))
            throw new InvalidOperationException("Not enough mana to perform action.");
        MP -= amount;
    }

    /// <summary>
    /// Восстанавливает ману
    /// </summary>
    public void Restore(int amount)
    {
        MP = Math.Max(0, Math.Min(MaxMP, MP + amount));
    }

    /// <summary>
    /// Очищает все фигуры участника
    /// </summary>
    public void ClearPieces()
    {
        Pieces.Clear();
    }

    /// <summary>
    /// Добавляет фигуру в коллекцию
    /// </summary>
    public void AddPiece(Piece piece)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        Pieces.Add(piece);
    }

    /// <summary>
    /// Проверяет, является ли участник ИИ
    /// </summary>
    public abstract bool IsAI { get; }

    /// <summary>
    /// Получает команду участника
    /// </summary>
    public abstract Team Team { get; }
}
