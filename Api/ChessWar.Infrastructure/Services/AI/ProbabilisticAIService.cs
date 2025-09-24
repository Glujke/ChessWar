using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.AI;
using Microsoft.Extensions.Logging;

namespace ChessWar.Infrastructure.Services.AI;

/// <summary>
/// Реализация вероятностного ИИ для Infrastructure слоя
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
    
    public bool MakeAiTurn(GameSession session)
    {
        Console.WriteLine($"[ProbabilisticAI] MakeAiTurn called");
        var turn = session.GetCurrentTurn();
        var active = turn.ActiveParticipant;
        Console.WriteLine($"[ProbabilisticAI] Active player: {active.Name} (ID: {active.Id})");
        Console.WriteLine($"[ProbabilisticAI] Active player pieces count: {active.Pieces.Count}");
        
        if (active.Pieces.Count == 0)
        {
            _logger.LogWarning("[ProbabilisticAI] No pieces available for AI turn");
            Console.WriteLine($"[ProbabilisticAI] No pieces available for AI turn - returning false");
            return false;
        }
        
        var difficulty = _difficultyProvider.GetDifficultyLevel(active);
        _logger.LogInformation("[ProbabilisticAI] Making turn with difficulty: {Difficulty}", difficulty);
        
        var markovAI = new MarkovDecisionAI(_probabilityMatrix, _evaluator, _difficultyProvider, _turnService, _abilityService);
        
        Console.WriteLine($"[ProbabilisticAI] Checking if Markov AI can execute turn");
        try
        {
            var canExecute = markovAI.CanExecute(session, turn, active);
            Console.WriteLine($"[ProbabilisticAI] Markov AI CanExecute result: {canExecute}");
            
            if (!canExecute)
            {
                _logger.LogWarning("[ProbabilisticAI] Markov AI cannot execute turn");
                Console.WriteLine($"[ProbabilisticAI] Markov AI cannot execute turn - returning false");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProbabilisticAI] Exception in CanExecute: {ex.Message}");
            Console.WriteLine($"[ProbabilisticAI] Exception stack trace: {ex.StackTrace}");
            return false;
        }
        
        Console.WriteLine($"[ProbabilisticAI] Calling Markov AI Execute");
        var success = false;
        try
        {
            success = markovAI.Execute(session, turn, active);
            Console.WriteLine($"[ProbabilisticAI] Markov AI Execute result: {success}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ProbabilisticAI] Exception in Execute: {ex.Message}");
            Console.WriteLine($"[ProbabilisticAI] Exception stack trace: {ex.StackTrace}");
            return false;
        }
        
        if (success)
        {
            _logger.LogInformation("[ProbabilisticAI] Turn executed successfully with {Difficulty} difficulty", difficulty);
            if (string.Equals(session.Mode, "Test", StringComparison.OrdinalIgnoreCase))
            {
                session.EndCurrentTurn();
            }
        }
        else
        {
            _logger.LogWarning("[ProbabilisticAI] Failed to execute turn with {Difficulty} difficulty", difficulty);
        }
        
        return success;
    }
    
    public async Task<bool> MakeAiTurnAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(MakeAiTurn(session));
    }
}
