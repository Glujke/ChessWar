using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Services.GameLogic;

/// <summary>
/// Реализация сервиса для проверки правил атак в игре Chess War
/// </summary>
public class AttackRulesService : IAttackRulesService
{
    private const int BoardSize = 8;
    private const int MinPosition = 0;
    private const int MaxPosition = BoardSize - 1;
    
    public bool CanAttack(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces)
    {
        if (!HasValidTarget(attacker, targetPosition, boardPieces))
            return false;
        
        if (!IsWithinAttackRange(attacker, targetPosition, boardPieces))
            return false;
        
        return true;
    }

    /// <summary>
    /// Проверяет, есть ли валидная цель для атаки
    /// </summary>
    public bool HasValidTarget(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces)
    {
        var targetPiece = boardPieces.FirstOrDefault(p => p.Position.Equals(targetPosition));
        
        if (targetPiece == null)
        {
            return false;
        }
        
        if (!targetPiece.IsAlive)
        {
            return false; 
        }
        
        if (!IsEnemy(attacker, targetPiece))
        {
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Проверяет, может ли фигура атаковать позицию по радиусу и правилам (без проверки цели)
    /// </summary>
    public bool IsWithinAttackRange(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces)
    {
        if (!IsValidPosition(targetPosition))
        {
            return false;
        }

        if (attacker.Position.Equals(targetPosition))
        {
            return false;
        }

        var distance = CalculateChebyshevDistance(attacker.Position, targetPosition);
        if (!IsWithinAttackRadius(attacker.Type, distance))
        {
            return false;
        }

        if (attacker.Type == PieceType.Pawn)
        {
            var dx = Math.Abs(targetPosition.X - attacker.Position.X);
            var dy = targetPosition.Y - attacker.Position.Y;
            var isElves = attacker.Team == Team.Elves;
            var forwardStep = isElves ? 1 : -1;
            var isForward = dy == forwardStep;
            var isStraightOrDiag = dx == 0 || dx == 1;
            if (!(isForward && isStraightOrDiag))
            {
                return false;
            }
        }

        if (!IsPathClear(attacker, targetPosition, boardPieces))
        {
            return false;
        }

        return true;
    }

    public IEnumerable<Position> GetAttackablePositions(Piece attacker, IEnumerable<Piece> boardPieces)
    {
        var maxRange = GetMaxAttackRange(attacker.Type);
        var boardPiecesList = boardPieces.ToList(); 

        for (int x = MinPosition; x < BoardSize; x++)
        {
            for (int y = MinPosition; y < BoardSize; y++)
            {
                var position = new Position(x, y);
                if (CanAttack(attacker, position, boardPiecesList))
                {
                    yield return position; 
                }
            }
        }
    }

    public List<Position> GetAvailableAttacks(Piece attacker, List<Piece> allPieces)
    {
        return GetAttackablePositions(attacker, allPieces).ToList();
    }

    public bool IsEnemy(Piece attacker, Piece target)
    {
        return attacker.Team != target.Team;
    }

    public int CalculateChebyshevDistance(Position from, Position to)
    {
        return Math.Max(Math.Abs(from.X - to.X), Math.Abs(from.Y - to.Y));
    }

    public bool IsPathClear(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces)
    {
        var from = attacker.Position;
        var to = targetPosition;

        if (CalculateChebyshevDistance(from, to) <= 1)
            return true;

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

        var occupiedPositions = new HashSet<Position>(boardPieces.Select(p => p.Position));

        for (int i = 1; i < steps; i++)
        {
            var x = from.X + (dx * i) / steps;
            var y = from.Y + (dy * i) / steps;
            var checkPosition = new Position(x, y);

            if (occupiedPositions.Contains(checkPosition))
                return false;
        }

        return true;
    }

    private bool IsValidPosition(Position position)
    {
        return position.X >= MinPosition && position.X < BoardSize && 
               position.Y >= MinPosition && position.Y < BoardSize;
    }

    private bool IsWithinAttackRadius(PieceType pieceType, int distance)
    {
        return distance <= GetMaxAttackRange(pieceType);
    }

    private int GetMaxAttackRange(PieceType pieceType)
    {
        return pieceType switch
        {
            PieceType.Pawn => 1,
            PieceType.Knight => 1,
            PieceType.Bishop => 4,
            PieceType.Rook => 8,
            PieceType.Queen => 3,
            PieceType.King => 1,
            _ => 0
        };
    }
}
