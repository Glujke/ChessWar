using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Исполнитель действий для ИИ
/// </summary>
public class ActionExecutor : IActionExecutor
{
    private readonly ITurnService _turnService;
    private readonly IAbilityService _abilityService;

    public ActionExecutor(ITurnService turnService, IAbilityService abilityService)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
    }

    public bool ExecuteActions(GameSession session, Turn turn, List<GameAction> actions)
    {
        if (!actions.Any())
        {
            return false;
        }

        var successCount = 0;

        foreach (var action in actions)
        {
            try
            {
                var success = ExecuteAction(session, turn, action);
                if (success)
                {
                    successCount++;
                }

                if (turn.RemainingMP <= 0)
                {
                    break;
                }
            }
            catch
            {
            }
        }

        return successCount > 0;
    }

    private bool ExecuteAction(GameSession session, Turn turn, GameAction action)
    {
        var piece = session.GetPieceById(action.PieceId);
        if (piece == null || !piece.IsAlive)
        {
            return false;
        }

        return action.Type switch
        {
            "Move" => _turnService.ExecuteMove(session, turn, piece, action.TargetPosition),
            "Attack" => _turnService.ExecuteAttack(session, turn, piece, action.TargetPosition),
            "Ability" => ExecuteAbility(session, turn, piece, action),
            _ => false
        };
    }

    private bool ExecuteAbility(GameSession session, Turn turn, Piece piece, GameAction action)
    {
        try
        {
            if (string.IsNullOrEmpty(action.AbilityName))
            {
                return false;
            }

            // Получаем все фигуры на доске
            var allPieces = session.GetAllPieces().ToList();
            
            // Выполняем способность
            return _abilityService.UseAbility(piece, action.AbilityName, action.TargetPosition, allPieces);
        }
        catch
        {
            return false;
        }
    }
}
