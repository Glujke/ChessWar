using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Configuration; using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Services.Common;
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
    private readonly IAbilityTargetService _abilityTargetService;
    private readonly IBoardContextService _boardContextService;

    public ActionQueryService(
        IPlayerManagementService playerManagementService,
        IMovementRulesService movementRulesService,
        IAttackRulesService attackRulesService,
        IAbilityTargetService abilityTargetService,
        IBoardContextService boardContextService)
    {
        _playerManagementService = playerManagementService ?? throw new ArgumentNullException(nameof(playerManagementService));
        _movementRulesService = movementRulesService ?? throw new ArgumentNullException(nameof(movementRulesService));
        _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
        _abilityTargetService = abilityTargetService ?? throw new ArgumentNullException(nameof(abilityTargetService));
        _boardContextService = boardContextService ?? throw new ArgumentNullException(nameof(boardContextService));
    }

    public async Task<List<PositionDto>> GetAvailableActionsAsync(GameSession gameSession, string pieceId, string actionType, string? abilityName = null, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (string.IsNullOrWhiteSpace(pieceId))
            throw new ArgumentException("Piece ID cannot be empty", nameof(pieceId));

        var currentTurn = gameSession.GetCurrentTurn();
        var piece = _playerManagementService.FindPieceById(gameSession, pieceId);

        if (piece == null)
            return new List<PositionDto>();

        var allPieces = await _boardContextService.GetAlivePiecesAsync(gameSession, cancellationToken);

        var ability = abilityName ?? string.Empty;

        var allPiecesList = allPieces.ToList();
        var actions = actionType switch
        {
            "Move" => _movementRulesService.GetAvailableMoves(piece, allPiecesList)
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList(),
            "Attack" => _attackRulesService.GetAvailableAttacks(piece, allPiecesList)
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList(),
            "Ability" => string.IsNullOrWhiteSpace(ability)
                ? new List<PositionDto>()
                : _abilityTargetService.GetAvailableTargets(piece, ability, allPiecesList)
                    .Select(p => new PositionDto { X = p.X, Y = p.Y })
                    .ToList(),
            _ => new List<PositionDto>()
        };

        return await Task.FromResult(actions);
    }
}
