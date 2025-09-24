using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Configuration; using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис получения доступных действий
/// </summary>
public class ActionQueryService : IActionQueryService
{
    private readonly IPlayerManagementService _playerManagementService;
    private readonly IMovementRulesService _movementRulesService;
    private readonly IAttackRulesService _attackRulesService;

    public ActionQueryService(
        IPlayerManagementService playerManagementService,
        IMovementRulesService movementRulesService,
        IAttackRulesService attackRulesService)
    {
        _playerManagementService = playerManagementService ?? throw new ArgumentNullException(nameof(playerManagementService));
        _movementRulesService = movementRulesService ?? throw new ArgumentNullException(nameof(movementRulesService));
        _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
    }

    public async Task<List<PositionDto>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (string.IsNullOrWhiteSpace(pieceId))
            throw new ArgumentException("Piece ID cannot be empty", nameof(pieceId));

        var currentTurn = gameSession.GetCurrentTurn();
        var piece = _playerManagementService.FindPieceById(gameSession, pieceId);

        if (piece == null)
            return new List<PositionDto>();

        var allPieces = gameSession.Board?.Pieces?.Where(p => p.IsAlive).ToList() ?? new List<Piece>();

        var actions = actionType switch
        {
            "Move" => _movementRulesService.GetAvailableMoves(piece, allPieces)
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList(),
            "Attack" => _attackRulesService.GetAvailableAttacks(piece, allPieces)
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList(),
            _ => new List<PositionDto>()
        };

        return await Task.FromResult(actions);
    }
}
