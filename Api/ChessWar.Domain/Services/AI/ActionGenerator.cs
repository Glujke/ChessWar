using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Генератор действий для ИИ
/// </summary>
public class ActionGenerator : IActionGenerator
{
    private readonly ITurnService _turnService;
    private readonly IAbilityService _abilityService;
    private readonly ILogger<ActionGenerator> _logger;

    public ActionGenerator(ITurnService turnService, IAbilityService abilityService, ILogger<ActionGenerator> logger)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public List<GameAction> GenerateActions(GameSession session, Turn turn, Participant active)
    {
        var actions = new List<GameAction>();

        if (!active.IsAI)
        {
            return actions;
        }

        var filteredPieces = session.GetBoard().GetAlivePiecesByTeam(active.Team);

        foreach (var piece in filteredPieces)
        {
            if (piece.Owner == null || piece.Owner.Id != active.Id)
            {
                continue;
            }

            var moves = GenerateMoveActions(session, turn, piece);
            actions.AddRange(moves);

            var attacks = GenerateAttackActions(session, turn, piece);
            actions.AddRange(attacks);

            var abilities = GenerateAbilityActions(session, turn, piece);
            actions.AddRange(abilities);
        }

        return actions;
    }

    private List<GameAction> GenerateMoveActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();

        try
        {
            var legalMoves = _turnService.GetAvailableMoves(session, turn, piece) ?? new List<Position>();
            foreach (var pos in legalMoves)
            {
                actions.Add(new GameAction
                {
                    Type = "Move",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = new Position(pos.X, pos.Y)
                });
            }
        }
        catch
        {
        }

        return actions;
    }

    private List<GameAction> GenerateAttackActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();

        try
        {
            var attacks = _turnService.GetAvailableAttacks(turn, piece);
            foreach (var pos in attacks)
            {
                actions.Add(new GameAction
                {
                    Type = "Attack",
                    PieceId = piece.Id.ToString(),
                    TargetPosition = new Position(pos.X, pos.Y)
                });
            }
        }
        catch
        {
        }

        return actions;
    }

    private List<GameAction> GenerateAbilityActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();

        return actions;
    }
}
