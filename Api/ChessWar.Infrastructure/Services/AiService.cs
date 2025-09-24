using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.AI;
using Microsoft.Extensions.Logging;

namespace ChessWar.Infrastructure.Services;

/// <summary>
/// Сервис ИИ для выполнения ходов
/// </summary>
public class AiService : IAIService
{
    private readonly IProbabilityMatrix _probabilityMatrix;
    private readonly IGameStateEvaluator _evaluator;
    private readonly IAIDifficultyLevel _difficultyProvider;
    private readonly ITurnService _turnService;
    private readonly IAbilityService _abilityService;
    private readonly ILogger<AiService> _logger;

    public AiService(
        IProbabilityMatrix probabilityMatrix,
        IGameStateEvaluator evaluator,
        IAIDifficultyLevel difficultyProvider,
        ITurnService turnService,
        IAbilityService abilityService,
        ILogger<AiService> logger)
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
        var turn = session.GetCurrentTurn();
        var active = turn.ActiveParticipant;
        if (active.Pieces.Count == 0) return false;

        _logger.LogInformation("[AI] Starting AI turn for player {PlayerId}", active.Id);
        _logger.LogInformation("[AI] Active player has {PieceCount} pieces", active.Pieces.Count(p => p.IsAlive));
        foreach (var piece in active.Pieces.Where(p => p.IsAlive))
        {
            _logger.LogInformation("[AI] Piece {PieceType} at ({X},{Y})", piece.Type, piece.Position.X, piece.Position.Y);
        }

        var markovAI = new MarkovDecisionAI(_probabilityMatrix, _evaluator, _difficultyProvider, _turnService, _abilityService);
        
        if (!markovAI.CanExecute(session, turn, active))
        {
            _logger.LogWarning("[AI] Markov AI cannot execute turn");
        return false;
        }

        var result = markovAI.Execute(session, turn, active);
        _logger.LogInformation("[AI] Markov AI result: {Result}", result);
        
        return result;
    }

    public async Task<bool> MakeAiTurnAsync(GameSession session, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(MakeAiTurn(session));
    }
}