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
    private readonly IAbilityTargetService _abilityTargetService;

    public ActionQueryService(
        IPlayerManagementService playerManagementService,
        IMovementRulesService movementRulesService,
        IAttackRulesService attackRulesService,
        IAbilityTargetService abilityTargetService)
    {
        _playerManagementService = playerManagementService ?? throw new ArgumentNullException(nameof(playerManagementService));
        _movementRulesService = movementRulesService ?? throw new ArgumentNullException(nameof(movementRulesService));
        _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
        _abilityTargetService = abilityTargetService ?? throw new ArgumentNullException(nameof(abilityTargetService));
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

        var allPieces = gameSession.Board?.Pieces?.Where(p => p.IsAlive).ToList() ?? new List<Piece>();

        var ability = abilityName ?? string.Empty;

        var actions = actionType switch
        {
            "Move" => _movementRulesService.GetAvailableMoves(piece, allPieces)
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList(),
            "Attack" => _attackRulesService.GetAvailableAttacks(piece, allPieces)
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList(),
            "Ability" => string.IsNullOrWhiteSpace(ability)
                ? new List<PositionDto>()
                : _abilityTargetService.GetAvailableTargets(piece, ability, allPieces)
                    .Select(p => new PositionDto { X = p.X, Y = p.Y })
                    .ToList(),
            _ => new List<PositionDto>()
        };

        return await Task.FromResult(actions);
    }
}
