using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Селектор действий для ИИ
/// </summary>
public class ActionSelector : IActionSelector
{
    private readonly IProbabilityMatrix _probabilityMatrix;
    private readonly IGameStateEvaluator _evaluator;
    private readonly IAIDifficultyLevel _difficultyProvider;

    public ActionSelector(
        IProbabilityMatrix probabilityMatrix,
        IGameStateEvaluator evaluator,
        IAIDifficultyLevel difficultyProvider)
    {
        _probabilityMatrix = probabilityMatrix ?? throw new ArgumentNullException(nameof(probabilityMatrix));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _difficultyProvider = difficultyProvider ?? throw new ArgumentNullException(nameof(difficultyProvider));
    }

    public List<GameAction> SelectActions(GameSession session, Turn turn, Participant active, List<GameAction> availableActions)
    {
        if (!availableActions.Any())
        {
            return new List<GameAction>();
        }

        var difficulty = _difficultyProvider.GetDifficultyLevel(active);
        var selectedActions = new List<GameAction>();

        var maxActions = GetMaxActionsForDifficulty(difficulty);
        var actionsToTake = System.Math.Min(maxActions, availableActions.Count);

        for (int i = 0; i < actionsToTake; i++)
        {
            selectedActions.Add(availableActions[i]);
        }

        return selectedActions;
    }

    private int GetMaxActionsForDifficulty(AIDifficultyLevel difficulty)
    {
        return difficulty switch
        {
            AIDifficultyLevel.Easy => 1,
            AIDifficultyLevel.Medium => 2,
            AIDifficultyLevel.Hard => 3,
            _ => 1
        };
    }
}
