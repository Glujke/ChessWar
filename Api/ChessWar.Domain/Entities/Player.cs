using ChessWar.Domain.Enums;

namespace ChessWar.Domain.Entities;

/// <summary>
/// Игрок в системе Chess War
/// </summary>
public class Player : Participant
{
    private readonly Team _team;

    public Player(string name, List<Piece> pieces) : base(name)
    {
        Pieces = pieces ?? throw new ArgumentNullException(nameof(pieces));
        _team = pieces.FirstOrDefault()?.Team ?? Team.Elves;
    }

    /// <summary>
    /// Конструктор для создания игрока с командой (для Tutorial)
    /// </summary>
    public Player(string name, Team team) : base(name)
    {
        _team = team;
    }

    public override bool IsAI => false;

    public override Team Team => _team;


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

}
