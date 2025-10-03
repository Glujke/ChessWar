using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.AI;

/// <summary>
/// Селектор действий для ИИ
/// </summary>
/// <summary>
/// Выбирает наиболее ценные действия ИИ на основе вероятностей, вознаграждений и оценки состояния.
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

    /// <summary>
    /// Возвращает список отобранных действий ИИ, отсортированных по ценности в пределах лимита сложности.
    /// </summary>
    public List<GameAction> SelectActions(GameSession session, Turn turn, Participant active, List<GameAction> availableActions)
    {
        if (!availableActions.Any())
        {
            return new List<GameAction>();
        }

        var difficulty = _difficultyProvider.GetDifficultyLevel(active);

        var scoredActions = availableActions
            .Select(action => new
            {
                Action = action,
                Score = CalculateActionScore(session, turn, active, action)
            })
            .OrderByDescending(x => x.Score)
            .ToList();

        var maxActions = GetMaxActionsForDifficulty(difficulty);
        var actionsToTake = System.Math.Min(maxActions, scoredActions.Count);

        return scoredActions
            .Take(actionsToTake)
            .Select(x => x.Action)
            .ToList();
    }

    /// <summary>
    /// Рассчитывает интегральную оценку действия для приоритезации.
    /// </summary>
    private double CalculateActionScore(GameSession session, Turn turn, Participant active, GameAction action)
    {
        var probability = _probabilityMatrix.GetActionProbability(session, action);
        var reward = _probabilityMatrix.GetReward(session, action);

        var stateEvaluation = _evaluator.EvaluateGameState(session, active);

        return probability * reward + stateEvaluation * 0.1;
    }

    /// <summary>
    /// Возвращает максимально допустимое число действий для уровня сложности.
    /// </summary>
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
