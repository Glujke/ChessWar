using ChessWar.Application.Interfaces.GameModes;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Application.Services.Common;
using ChessWar.Application.Interfaces.Configuration;
using AutoMapper;

namespace ChessWar.Application.Services.GameModes;

/// <summary>
/// Фабрика для создания стратегий игровых режимов
/// </summary>
public class GameModeStrategyFactory : IGameModeStrategyFactory
{
    private readonly IGameSessionManagementService _sessionManagementService;
    private readonly IActionExecutionService _actionExecutionService;
    private readonly ITurnOrchestrator _turnOrchestrator;
    private readonly IAITurnService _aiTurnService;
    private readonly IActionQueryService _actionQueryService;
    private readonly ITutorialService _tutorialService;
    private readonly IBattlePresetService _battlePresetService;
    private readonly IMapper _mapper;

    public GameModeStrategyFactory(
        IGameSessionManagementService sessionManagementService,
        IActionExecutionService actionExecutionService,
        ITurnOrchestrator turnOrchestrator,
        IAITurnService aiTurnService,
        IActionQueryService actionQueryService,
        ITutorialService tutorialService,
        IBattlePresetService battlePresetService,
        IMapper mapper)
    {
        _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
        _turnOrchestrator = turnOrchestrator ?? throw new ArgumentNullException(nameof(turnOrchestrator));
        _aiTurnService = aiTurnService ?? throw new ArgumentNullException(nameof(aiTurnService));
        _actionQueryService = actionQueryService ?? throw new ArgumentNullException(nameof(actionQueryService));
        _tutorialService = tutorialService ?? throw new ArgumentNullException(nameof(tutorialService));
        _battlePresetService = battlePresetService ?? throw new ArgumentNullException(nameof(battlePresetService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public IGameModeStrategy GetStrategy(string mode)
    {
        return mode.ToLowerInvariant() switch
        {
            "tutorial" => new TutorialModeStrategy(
                _sessionManagementService,
                _actionExecutionService,
                _turnOrchestrator,
                _aiTurnService,
                _actionQueryService,
                _tutorialService,
                _battlePresetService,
                _mapper),
            "ai" => new AIModeStrategy(
                _sessionManagementService,
                _actionExecutionService,
                _turnOrchestrator,
                _aiTurnService,
                _actionQueryService,
                _tutorialService,
                _mapper),
            "local" => new LocalModeStrategy(
                _sessionManagementService,
                _actionExecutionService,
                _turnOrchestrator,
                _aiTurnService,
                _actionQueryService,
                _tutorialService,
                _mapper),
            "online" => new OnlineModeStrategy(
                _sessionManagementService,
                _actionExecutionService,
                _turnOrchestrator,
                _aiTurnService,
                _actionQueryService,
                _tutorialService,
                _mapper),
            "localcoop" => new LocalModeStrategy(
                _sessionManagementService,
                _actionExecutionService,
                _turnOrchestrator,
                _aiTurnService,
                _actionQueryService,
                _tutorialService,
                _mapper),
            _ => throw new ArgumentException($"Unsupported game mode: {mode}")
        };
    }

    public IGameModeStrategy? TryGetStrategy(string mode)
    {
        try
        {
            return GetStrategy(mode);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public bool IsModeSupported(string mode)
    {
        return mode.ToLowerInvariant() switch
        {
            "tutorial" or "ai" or "local" or "online" => true,
            _ => false
        };
    }

    public IEnumerable<string> GetSupportedModes()
    {
        return new[] { "Tutorial", "AI", "Local", "Online" };
    }
}
