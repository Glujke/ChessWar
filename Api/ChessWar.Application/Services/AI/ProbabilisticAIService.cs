using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.AI;
using Microsoft.Extensions.Logging;

namespace ChessWar.Application.Services.AI;

/// <summary>
/// Сервис вероятностного ИИ для Application слоя
/// </summary>
public class ProbabilisticAIService : IAIService
{
    private readonly IProbabilityMatrix _probabilityMatrix;
    private readonly IGameStateEvaluator _evaluator;
    private readonly IAIDifficultyLevel _difficultyProvider;
    private readonly ITurnService _turnService;
    private readonly IAbilityService _abilityService;
    private readonly ILogger<ProbabilisticAIService> _logger;
    
    public ProbabilisticAIService(
        IProbabilityMatrix probabilityMatrix,
        IGameStateEvaluator evaluator,
        IAIDifficultyLevel difficultyProvider,
        ITurnService turnService,
        IAbilityService abilityService,
        ILogger<ProbabilisticAIService> logger)
    {
        _probabilityMatrix = probabilityMatrix ?? throw new ArgumentNullException(nameof(probabilityMatrix));
        _evaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        _difficultyProvider = difficultyProvider ?? throw new ArgumentNullException(nameof(difficultyProvider));
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <summary>
    /// Выполнить ход ИИ с использованием вероятностных стратегий
    /// </summary>
    public bool MakeAiTurn(GameSession session)
    {
        var turn = session.GetCurrentTurn();
        var active = turn.ActiveParticipant;
        
        if (active.Pieces.Count == 0)
        {
            _logger.LogWarning("[AI] No pieces available for AI turn");
            return false;
        }
        
        var difficulty = _difficultyProvider.GetDifficultyLevel(active);
        _logger.LogInformation("[AI] Making turn with difficulty: {Difficulty}", difficulty);
        
        var markovAI = new MarkovDecisionAI(_probabilityMatrix, _evaluator, _difficultyProvider, _turnService, _abilityService);
        
        if (!markovAI.CanExecute(session, turn, active))
        {
            _logger.LogWarning("[AI] Markov AI cannot execute turn");
            return false;
        }
        
        var success = markovAI.Execute(session, turn, active);
        
        if (success)
        {
            _logger.LogInformation("[AI] Turn executed successfully with {Difficulty} difficulty", difficulty);
        }
        else
        {
            _logger.LogWarning("[AI] Failed to execute turn with {Difficulty} difficulty", difficulty);
        }
        
        return success;
    }
    
    /// <summary>
    /// Выполнить ход ИИ асинхронно
    /// </summary>
    public async Task<bool> MakeAiTurnAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => MakeAiTurn(session), cancellationToken);
    }
    
    /// <summary>
    /// Обучить ИИ на основе результатов игры
    /// </summary>
    public void LearnFromGame(GameSession session, object result)
    {
        _logger.LogInformation("[AI] Learning from game result: {Result}", result);
    }
    
    /// <summary>
    /// Получить оценку текущего состояния игры
    /// </summary>
    public double EvaluateGameState(GameSession session, Player player)
    {
        return _evaluator.EvaluateGameState(session, player);
    }
}