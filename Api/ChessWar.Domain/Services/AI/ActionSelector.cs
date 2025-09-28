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
        
        // Оцениваем и сортируем действия по их ценности
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

    private double CalculateActionScore(GameSession session, Turn turn, Participant active, GameAction action)
    {
        // Базовая оценка из матрицы вероятностей
        var probability = _probabilityMatrix.GetActionProbability(session, action);
        var reward = _probabilityMatrix.GetReward(session, action);
        
        // Оценка состояния игры
        var stateEvaluation = _evaluator.EvaluateGameState(session, active);
        
        // Комбинированная оценка
        return probability * reward + stateEvaluation * 0.1;
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
