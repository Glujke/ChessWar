using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using Microsoft.Extensions.Logging;

namespace ChessWar.Domain.Services.GameLogic;

public class MovementRulesService : IMovementRulesService
{
    private readonly ILogger<MovementRulesService> _logger;

    public MovementRulesService(ILogger<MovementRulesService> logger)
    {
        _logger = logger;
    }

    public bool CanMoveTo(Piece piece, Position targetPosition, IReadOnlyList<Piece> boardPieces)
    {
        _logger.LogDebug("[MovementRules] CanMoveTo: piece {PieceId} ({PieceType}) at ({FromX},{FromY}) to ({ToX},{ToY})", 
            piece.Id, piece.Type, piece.Position.X, piece.Position.Y, targetPosition.X, targetPosition.Y);
        
        if (!IsValidPosition(targetPosition))
        {
            _logger.LogDebug("[MovementRules] Invalid position: ({X},{Y})", targetPosition.X, targetPosition.Y);
            return false;
        }

        if (piece.Position == targetPosition)
        {
            _logger.LogDebug("[MovementRules] Cannot move to same position");
            return false;
        }

        var targetPiece = boardPieces.FirstOrDefault(p => p.Position == targetPosition);
        if (targetPiece != null && targetPiece.Team == piece.Team)
        {
            _logger.LogDebug("[MovementRules] Target position occupied by ally: piece {TargetPieceId} (Team: {TargetTeam}) at ({X},{Y})", 
                targetPiece.Id, targetPiece.Team, targetPiece.Position.X, targetPiece.Position.Y);
            return false;
        }

        var result = piece.Type switch
        {
            PieceType.Pawn => CanPawnMove(piece, targetPosition, boardPieces),
            PieceType.Knight => CanKnightMove(piece, targetPosition, boardPieces),
            PieceType.Bishop => CanBishopMove(piece, targetPosition, boardPieces),
            PieceType.Rook => CanRookMove(piece, targetPosition, boardPieces),
            PieceType.Queen => CanQueenMove(piece, targetPosition, boardPieces),
            PieceType.King => CanKingMove(piece, targetPosition, boardPieces),
            _ => false
        };
        
        _logger.LogDebug("[MovementRules] CanMoveTo result: {Result}", result);
        return result;
    }

    public IReadOnlyList<Position> GetPossibleMoves(Piece piece, IReadOnlyList<Piece> boardPieces)
    {
        var possibleMoves = new List<Position>();

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                var targetPosition = new Position(x, y);
                if (CanMoveTo(piece, targetPosition, boardPieces))
                {
                    possibleMoves.Add(targetPosition);
                }
            }
        }

        return possibleMoves;
    }

    public List<Position> GetAvailableMoves(Piece piece, List<Piece> allPieces)
    {
        return GetPossibleMoves(piece, allPieces).ToList();
    }

    public bool IsValidPosition(Position position)
    {
        return position.X >= 0 && position.X < 8 && 
               position.Y >= 0 && position.Y < 8;
    }

    private bool CanPawnMove(Piece pawn, Position target, IReadOnlyList<Piece> boardPieces)
    {
        var dx = target.X - pawn.Position.X;
        var dy = target.Y - pawn.Position.Y;

        _logger.LogDebug("[MovementRules] CanPawnMove: pawn at ({PawnX},{PawnY}), target ({TargetX},{TargetY})", 
            pawn.Position.X, pawn.Position.Y, target.X, target.Y);
        _logger.LogDebug("[MovementRules] dx={Dx}, dy={Dy}, Team={Team}, IsFirstMove={IsFirstMove}", 
            dx, dy, pawn.Team, pawn.IsFirstMove);

        if (pawn.Team == Team.Elves)
        {
            _logger.LogDebug("[MovementRules] Elves pawn - checking forward movement");
            if (dx == 0 && dy == 1)
            {
                var isEmpty = IsEmpty(target, boardPieces);
                _logger.LogDebug("[MovementRules] Forward 1: dx={Dx}, dy={Dy}, isEmpty={IsEmpty}", dx, dy, isEmpty);
                if (isEmpty) return true;
            }

            if (dx == 0 && dy == 2 && pawn.IsFirstMove)
            {
                var isEmptyTarget = IsEmpty(target, boardPieces);
                var isEmptyMiddle = IsEmpty(new Position(target.X, target.Y - 1), boardPieces);
                _logger.LogDebug("[MovementRules] Forward 2: dx={Dx}, dy={Dy}, isEmptyTarget={IsEmptyTarget}, isEmptyMiddle={IsEmptyMiddle}", dx, dy, isEmptyTarget, isEmptyMiddle);
                if (isEmptyTarget && isEmptyMiddle) return true;
            }

        }
        else
        {
            _logger.LogDebug("[MovementRules] Orcs pawn - checking downward movement");
            if (dx == 0 && dy == -1)
            {
                var isEmpty = IsEmpty(target, boardPieces);
                _logger.LogDebug("[MovementRules] Downward 1: dx={Dx}, dy={Dy}, isEmpty={IsEmpty}", dx, dy, isEmpty);
                if (isEmpty) return true;
            }

            if (dx == 0 && dy == -2 && pawn.IsFirstMove)
            {
                var isEmptyTarget = IsEmpty(target, boardPieces);
                var isEmptyMiddle = IsEmpty(new Position(target.X, target.Y + 1), boardPieces);
                _logger.LogDebug("[MovementRules] Downward 2: dx={Dx}, dy={Dy}, isEmptyTarget={IsEmptyTarget}, isEmptyMiddle={IsEmptyMiddle}", dx, dy, isEmptyTarget, isEmptyMiddle);
                if (isEmptyTarget && isEmptyMiddle) return true;
            }
        }

        _logger.LogDebug("[MovementRules] CanPawnMove: NO VALID MOVE FOUND for piece {Id} at ({X},{Y}) to ({TX},{TY}) dx={Dx} dy={Dy} isFirst={IsFirst} team={Team}",
            pawn.Id, pawn.Position.X, pawn.Position.Y, target.X, target.Y, dx, dy, pawn.IsFirstMove, pawn.Team);
        return false;
    }

    private bool CanKnightMove(Piece knight, Position target, IReadOnlyList<Piece> boardPieces)
    {
        var dx = Math.Abs(target.X - knight.Position.X);
        var dy = Math.Abs(target.Y - knight.Position.Y);

        if ((dx == 2 && dy == 1) || (dx == 1 && dy == 2))
        {
            return IsEmpty(target, boardPieces) || IsOccupiedByEnemy(target, knight.Team, boardPieces);
        }

        return false;
    }

    private bool CanBishopMove(Piece bishop, Position target, IReadOnlyList<Piece> boardPieces)
    {
        var dx = target.X - bishop.Position.X;
        var dy = target.Y - bishop.Position.Y;

        if (Math.Abs(dx) == Math.Abs(dy) && dx != 0)
        {
            var stepX = dx > 0 ? 1 : -1;
            var stepY = dy > 0 ? 1 : -1;
            var steps = Math.Abs(dx);

            for (int i = 1; i < steps; i++)
            {
                var checkPos = new Position(bishop.Position.X + i * stepX, bishop.Position.Y + i * stepY);
                if (!IsEmpty(checkPos, boardPieces))
                    return false;
            }

            return IsEmpty(target, boardPieces) || IsOccupiedByEnemy(target, bishop.Team, boardPieces);
        }

        return false;
    }

    private bool CanRookMove(Piece rook, Position target, IReadOnlyList<Piece> boardPieces)
    {
        var dx = target.X - rook.Position.X;
        var dy = target.Y - rook.Position.Y;

        if ((dx == 0 && dy != 0) || (dx != 0 && dy == 0))
        {
            var stepX = dx == 0 ? 0 : (dx > 0 ? 1 : -1);
            var stepY = dy == 0 ? 0 : (dy > 0 ? 1 : -1);
            var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));

            for (int i = 1; i < steps; i++)
            {
                var checkPos = new Position(rook.Position.X + i * stepX, rook.Position.Y + i * stepY);
                if (!IsEmpty(checkPos, boardPieces))
                    return false;
            }

            return IsEmpty(target, boardPieces) || IsOccupiedByEnemy(target, rook.Team, boardPieces);
        }

        return false;
    }

    private bool CanQueenMove(Piece queen, Position target, IReadOnlyList<Piece> boardPieces)
    {
        return CanRookMove(queen, target, boardPieces) || CanBishopMove(queen, target, boardPieces);
    }

    private bool CanKingMove(Piece king, Position target, IReadOnlyList<Piece> boardPieces)
    {
        var dx = Math.Abs(target.X - king.Position.X);
        var dy = Math.Abs(target.Y - king.Position.Y);


        if (dx <= 1 && dy <= 1 && (dx + dy) > 0)
        {
            var isEmpty = IsEmpty(target, boardPieces);
            var isEnemy = IsOccupiedByEnemy(target, king.Team, boardPieces);
            var result = isEmpty || isEnemy;
            return result;
        }

        return false;
    }

    private bool IsEmpty(Position position, IReadOnlyList<Piece> boardPieces)
    {
        var isEmpty = !boardPieces.Any(p => p.Position == position);
        return isEmpty;
    }

    private bool IsOccupiedByEnemy(Position position, Team team, IReadOnlyList<Piece> boardPieces)
    {
        var piece = boardPieces.FirstOrDefault(p => p.Position == position);
        var isEnemy = piece != null && piece.Team != team;
        return isEnemy;
    }
}
