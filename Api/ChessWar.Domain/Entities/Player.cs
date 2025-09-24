using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Игрок в системе Chess War
/// </summary>
public class Player
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public List<Piece> Pieces { get; private set; }
    public int Victories { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int MP { get; private set; }
    public int MaxMP { get; private set; }

    public Player(string name, List<Piece> pieces)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Pieces = pieces ?? throw new ArgumentNullException(nameof(pieces));
        Victories = 0;
        CreatedAt = DateTime.UtcNow;
        MP = 0;
        MaxMP = 0;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Player name cannot be empty", nameof(name));
        }
    }

    /// <summary>
    /// Конструктор для создания игрока с командой (для Tutorial)
    /// </summary>
    public Player(string name, Team team)
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
            throw new ArgumentException("Player name cannot be empty", nameof(name));
        }
    }

    /// <summary>
    /// Добавляет победу игроку
    /// </summary>
    public void AddVictory()
    {
        Victories++;
    }

    public void SetMana(int current, int max)
    {
        if (max < 0) throw new ArgumentOutOfRangeException(nameof(max));
        if (current < 0) current = 0;
        MP = Math.Min(current, max);
        MaxMP = max;
    }

    public bool CanSpend(int amount)
    {
        if (amount < 0) return false;
        return MP >= amount;
    }

    public void Spend(int amount)
    {
        if (!CanSpend(amount))
            throw new InvalidOperationException("Not enough mana to perform action.");
        MP -= amount;
    }

    public void Restore(int amount)
    {
        MP = Math.Max(0, Math.Min(MaxMP, MP + amount));
    }

    /// <summary>
    /// Получает фигуры по типу
    /// </summary>
    public List<Piece> GetPiecesByType(PieceType type)
    {
        return Pieces.Where(p => p.Type == type).ToList();
    }

    /// <summary>
    /// Получает живые фигуры
    /// </summary>
    public List<Piece> GetAlivePieces()
    {
        return Pieces.Where(p => p.IsAlive).ToList();
    }

    /// <summary>
    /// Получает фигуры по команде
    /// </summary>
    public List<Piece> GetPiecesByTeam(Team team)
    {
        return Pieces.Where(p => p.Team == team).ToList();
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
    /// Удаляет фигуру из коллекции
    /// </summary>
    public void RemovePiece(Piece piece)
    {
        if (piece == null)
            return;

        Pieces.Remove(piece);
    }

    /// <summary>
    /// Удаляет мертвую фигуру из коллекции
    /// </summary>
    public void RemoveDeadPiece(Piece piece)
    {
        if (piece == null)
            return;

        if (!piece.IsAlive)
        {
            Pieces.Remove(piece);
        }
    }

    /// <summary>
    /// Очищает все фигуры игрока
    /// </summary>
    public void ClearPieces()
    {
        Pieces.Clear();
    }
}
