using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Entities;

public class GameBoard
{
    public const int Size = 8;
    private readonly Piece?[,] _board = new Piece[Size, Size];
    
    public List<Piece> Pieces => GetAllPieces();
    
    public Piece? GetPieceAt(Position position)
    {
        if (!IsValidPosition(position))
            return null;
        return _board[position.X, position.Y];
    }
    
    public void SetPieceAt(Position position, Piece? piece)
    {
        if (!IsValidPosition(position))
            return;
        _board[position.X, position.Y] = piece;
    }
    
    public void MovePiece(Piece piece, Position newPosition)
    {
        var oldPosition = piece.Position;
        
        
        if (oldPosition != null)
        {
            SetPieceAt(oldPosition, null);
        }
        
        SetPieceAt(newPosition, piece);
        
        piece.Position = newPosition;
        piece.IsFirstMove = false;
        
        Console.WriteLine($"[GameBoard] After move: piece {piece.Id} position is ({piece.Position.X},{piece.Position.Y})");
    }

    /// <summary>
    /// Размещает фигуру на доске
    /// </summary>
    public void PlacePiece(Piece piece)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        if (!IsValidPosition(piece.Position))
            throw new ArgumentException("Invalid position for piece", nameof(piece));

        SetPieceAt(piece.Position, piece);
    }

    /// <summary>
    /// Удаляет фигуру с доски
    /// </summary>
    public void RemovePiece(Piece piece)
    {
        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        if (IsValidPosition(piece.Position))
        {
            SetPieceAt(piece.Position, null);
        }
    }
    
    public bool IsValidPosition(Position position)
    {
        return position.X >= 0 && position.X < Size && 
               position.Y >= 0 && position.Y < Size;
    }
    
    public bool IsEmpty(Position position)
    {
        return GetPieceAt(position) == null;
    }
    
    public bool IsOccupied(Position position)
    {
        return !IsEmpty(position);
    }
    
    public List<Piece> GetAllPieces()
    {
        var pieces = new List<Piece>();
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                var piece = _board[x, y];
                if (piece != null)
                    pieces.Add(piece);
            }
        }
        return pieces;
    }
    
    public List<Piece> GetPiecesByTeam(Enums.Team team)
    {
        return GetAllPieces().Where(p => p.Team == team).ToList();
    }
    
    public List<Piece> GetAlivePieces()
    {
        return GetAllPieces().Where(p => p.IsAlive).ToList();
    }
    
    public List<Piece> GetAlivePiecesByTeam(Enums.Team team)
    {
        return GetAlivePieces().Where(p => p.Team == team).ToList();
    }
    
    /// <summary>
    /// Очищает доску
    /// </summary>
    public void Clear()
    {
        for (int x = 0; x < Size; x++)
        {
            for (int y = 0; y < Size; y++)
            {
                _board[x, y] = null;
            }
        }
    }
}
