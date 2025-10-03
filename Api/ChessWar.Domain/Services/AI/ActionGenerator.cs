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
/// <summary>
/// Формирует набор возможных действий ИИ (ходы, атаки, способности) для текущего состояния.
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

    /// <summary>
    /// Генерирует список доступных действий ИИ для активного участника.
    /// </summary>
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

    /// <summary>
    /// Создаёт действия перемещения для указанной фигуры.
    /// </summary>
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

    /// <summary>
    /// Создаёт действия атаки для указанной фигуры.
    /// </summary>
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

    /// <summary>
    /// Создаёт действия использования способностей для указанной фигуры.
    /// </summary>
    private List<GameAction> GenerateAbilityActions(GameSession session, Turn turn, Piece piece)
    {
        var actions = new List<GameAction>();

        try
        {
            var allPieces = session.GetAllPieces().ToList();

            var abilityNames = GetAvailableAbilityNames(piece);

            foreach (var abilityName in abilityNames)
            {
                var maxRange = 3;
                var startX = System.Math.Max(0, piece.Position.X - maxRange);
                var endX = System.Math.Min(7, piece.Position.X + maxRange);
                var startY = System.Math.Max(0, piece.Position.Y - maxRange);
                var endY = System.Math.Min(7, piece.Position.Y + maxRange);

                for (int x = startX; x <= endX; x++)
                {
                    for (int y = startY; y <= endY; y++)
                    {
                        var target = new Position(x, y);

                        if (_abilityService.CanUseAbility(piece, abilityName, target, allPieces))
                        {
                            actions.Add(new GameAction
                            {
                                Type = "Ability",
                                PieceId = piece.Id.ToString(),
                                TargetPosition = target,
                                AbilityName = abilityName
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate ability actions for piece {PieceId}", piece.Id);
        }

        return actions;
    }

    /// <summary>
    /// Возвращает список известных способностей для типа фигуры.
    /// </summary>
    private List<string> GetAvailableAbilityNames(Piece piece)
    {
        // Возвращаем известные способности для каждого типа фигуры
        return piece.Type switch
        {
            Enums.PieceType.Pawn => new List<string> { "ShieldBash", "Breakthrough" },
            Enums.PieceType.Knight => new List<string> { "DoubleStrike", "IronStance" },
            Enums.PieceType.Bishop => new List<string> { "LightArrow", "Heal" },
            Enums.PieceType.Rook => new List<string> { "ArrowStorm", "Fortress" },
            Enums.PieceType.Queen => new List<string> { "MagicBlast", "Resurrection" },
            Enums.PieceType.King => new List<string> { "RoyalCommand" },
            _ => new List<string>()
        };
    }
}
