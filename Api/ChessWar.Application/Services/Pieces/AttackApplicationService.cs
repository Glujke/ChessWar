using ChessWar.Application.Interfaces.Pieces; 
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Application.Services.Pieces;

/// <summary>
/// Реализация сервиса приложения для управления атаками
/// </summary>
public class AttackApplicationService : IAttackApplicationService
{
    private readonly IAttackRulesService _attackRulesService;
    private readonly IPieceService _pieceService;
    private readonly IBoardService _boardService;

    public AttackApplicationService(
        IAttackRulesService attackRulesService,
        IPieceService pieceService,
        IBoardService boardService)
    {
        _attackRulesService = attackRulesService;
        _pieceService = pieceService;
        _boardService = boardService;
    }

    public async Task<AttackApplicationResult> CheckAttackAsync(int attackerId, Position targetPosition, CancellationToken cancellationToken = default)
    {
        var attacker = await _pieceService.GetPieceByIdAsync(attackerId, cancellationToken);
        if (attacker == null)
        {
            return new AttackApplicationResult
            {
                CanAttack = false,
                Reason = $"Piece with ID {attackerId} not found"
            };
        }

        var board = await _boardService.GetBoardAsync();
        var boardPieces = board.Pieces;

        var canAttack = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        var distance = _attackRulesService.CalculateChebyshevDistance(attacker.Position, targetPosition);
        var maxRange = GetMaxAttackRange(attacker.Type);

        return new AttackApplicationResult
        {
            CanAttack = canAttack,
            Distance = distance,
            MaxRange = maxRange,
            Attacker = attacker,
            Reason = canAttack ? null : GetAttackFailureReason(attacker, targetPosition, boardPieces)
        };
    }

    public async Task<IEnumerable<Position>> GetAttackablePositionsAsync(int attackerId, CancellationToken cancellationToken = default)
    {
        var attacker = await _pieceService.GetPieceByIdAsync(attackerId, cancellationToken);
        if (attacker == null)
            return Enumerable.Empty<Position>();

        var board = await _boardService.GetBoardAsync();
        var boardPieces = board.Pieces;

        return _attackRulesService.GetAttackablePositions(attacker, boardPieces);
    }

    public async Task<bool> IsEnemyAsync(int attackerId, int targetId, CancellationToken cancellationToken = default)
    {
        var attacker = await _pieceService.GetPieceByIdAsync(attackerId, cancellationToken);
        if (attacker == null)
            return false;

        var target = await _pieceService.GetPieceByIdAsync(targetId, cancellationToken);
        if (target == null)
            return false;

        return _attackRulesService.IsEnemy(attacker, target);
    }

    public int CalculateChebyshevDistance(Position from, Position to)
    {
        return _attackRulesService.CalculateChebyshevDistance(from, to);
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

    private string? GetAttackFailureReason(Piece attacker, Position targetPosition, IEnumerable<Piece> boardPieces)
    {
        if (targetPosition.X < 0 || targetPosition.X >= 8 || targetPosition.Y < 0 || targetPosition.Y >= 8)
            return "Target position is outside board boundaries";

        if (attacker.Position.Equals(targetPosition))
            return "Cannot attack own position";

        var distance = _attackRulesService.CalculateChebyshevDistance(attacker.Position, targetPosition);
        var maxRange = GetMaxAttackRange(attacker.Type);
        if (distance > maxRange)
            return $"Target is too far (distance: {distance}, max range: {maxRange})";

        if (!_attackRulesService.IsPathClear(attacker, targetPosition, boardPieces))
            return "Path to target is blocked";

        var targetPiece = boardPieces.FirstOrDefault(p => p.Position.Equals(targetPosition));
        if (targetPiece != null && !_attackRulesService.IsEnemy(attacker, targetPiece))
            return "Target is an ally, cannot attack";

        return "Unknown reason";
    }
}
