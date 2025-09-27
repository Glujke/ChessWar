using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.GameModes;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Application.Services.Common;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using AutoMapper;

namespace ChessWar.Application.Services.GameModes;

/// <summary>
/// Базовая стратегия игрового режима с общими методами
/// </summary>
public abstract class BaseGameModeStrategy : IGameModeStrategy
{
    protected readonly IGameSessionManagementService _sessionManagementService;
    protected readonly IActionExecutionService _actionExecutionService;
    protected readonly ITurnOrchestrator _turnOrchestrator;
    protected readonly IAITurnService _aiTurnService;
    protected readonly IActionQueryService _actionQueryService;
    protected readonly ITutorialService _tutorialService;
    protected readonly IMapper _mapper;

    protected BaseGameModeStrategy(
        IGameSessionManagementService sessionManagementService,
        IActionExecutionService actionExecutionService,
        ITurnOrchestrator turnOrchestrator,
        IAITurnService aiTurnService,
        IActionQueryService actionQueryService,
        ITutorialService tutorialService,
        IMapper mapper)
    {
        _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
        _actionExecutionService = actionExecutionService ?? throw new ArgumentNullException(nameof(actionExecutionService));
        _turnOrchestrator = turnOrchestrator ?? throw new ArgumentNullException(nameof(turnOrchestrator));
        _aiTurnService = aiTurnService ?? throw new ArgumentNullException(nameof(aiTurnService));
        _actionQueryService = actionQueryService ?? throw new ArgumentNullException(nameof(actionQueryService));
        _tutorialService = tutorialService ?? throw new ArgumentNullException(nameof(tutorialService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public virtual async Task<GameSessionDto> CreateSessionAsync(CreateGameSessionDto dto)
    {
        var session = await _sessionManagementService.CreateGameSessionAsync(dto);
        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> ExecuteActionAsync(GameSession session, ExecuteActionDto dto)
    {
        var success = await _actionExecutionService.ExecuteActionAsync(session, dto);
        if (!success)
        {
            throw new InvalidOperationException("Action execution failed");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> EndTurnAsync(GameSession session)
    {
        await _turnOrchestrator.EndTurnAsync(session);
        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> MakeAiTurnAsync(GameSession session)
    {
        var success = await _aiTurnService.MakeAiTurnAsync(session);
        if (!success)
        {
            throw new InvalidOperationException("AI turn failed");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> MovePieceAsync(GameSession session, string pieceId, PositionDto targetPosition)
    {
        var piece = session.GetAllPieces().FirstOrDefault(p => p.Id.ToString() == pieceId);
        if (piece == null)
        {
            throw new ArgumentException($"Piece with ID {pieceId} not found");
        }

        var position = new Position(targetPosition.X, targetPosition.Y);
        var success = await _actionExecutionService.ExecuteMoveAsync(session, session.GetCurrentTurn(), piece, position);
        if (!success)
        {
            throw new InvalidOperationException("Move failed");
        }

        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<List<PositionDto>> GetAvailableActionsAsync(GameSession session, string pieceId, string actionType, string? abilityName = null)
    {
        return await _actionQueryService.GetAvailableActionsAsync(session, pieceId, actionType, abilityName);
    }

    public virtual async Task<GameSessionDto> ExecuteAbilityAsync(GameSession session, AbilityRequestDto request)
    {
        var dto = new ExecuteActionDto
        {
            Type = "Ability",
            PieceId = request.PieceId,
            TargetPosition = request.Target,
            Description = request.AbilityName
        };

        var success = await _actionExecutionService.ExecuteActionAsync(session, dto);
        if (!success)
        {
            throw new InvalidOperationException("Ability execution failed");
        }

        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> ExecuteEvolutionAsync(GameSession session, string pieceId, string targetType)
    {
        var dto = new ExecuteActionDto
        {
            Type = "Evolution",
            PieceId = pieceId,
            TargetPosition = null,
            Description = targetType
        };

        var success = await _actionExecutionService.ExecuteActionAsync(session, dto);
        if (!success)
        {
            throw new InvalidOperationException("Evolution failed");
        }

        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> TutorialTransitionAsync(GameSession session, TutorialTransitionRequestDto request)
    {
        throw new NotImplementedException("Tutorial transition is not implemented for this game mode");
    }

    public virtual async Task<GameSessionDto> GetSessionAsync(Guid sessionId)
    {
        var session = await _sessionManagementService.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} not found");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> StartGameAsync(GameSession session)
    {
        await _sessionManagementService.StartGameAsync(session);
        return _mapper.Map<GameSessionDto>(session);
    }

    public virtual async Task<GameSessionDto> CompleteGameAsync(GameSession session, GameResult result)
    {
        await _sessionManagementService.CompleteGameAsync(session, result);
        return _mapper.Map<GameSessionDto>(session);
    }
}
