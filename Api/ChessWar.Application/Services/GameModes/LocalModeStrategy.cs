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
/// Стратегия для Local режима (заглушка - в разработке)
/// </summary>
public class LocalModeStrategy : BaseGameModeStrategy
{
    public LocalModeStrategy(
        IGameSessionManagementService sessionManagementService,
        IActionExecutionService actionExecutionService,
        ITurnOrchestrator turnOrchestrator,
        IAITurnService aiTurnService,
        IActionQueryService actionQueryService,
        ITutorialService tutorialService,
        IMapper mapper)
        : base(sessionManagementService, actionExecutionService, turnOrchestrator, aiTurnService, actionQueryService, tutorialService, mapper)
    {
    }

    public override async Task<GameSessionDto> CreateSessionAsync(CreateGameSessionDto dto)
    {
        var session = await _sessionManagementService.CreateGameSessionAsync(dto);
        await _sessionManagementService.StartGameAsync(session);
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> ExecuteActionAsync(GameSession session, ExecuteActionDto dto)
    {
        var success = await _actionExecutionService.ExecuteActionAsync(session, dto);
        if (!success)
        {
            throw new ArgumentException("Action execution failed - invalid move or insufficient resources");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> EndTurnAsync(GameSession session)
    {
        var currentTurn = session.GetCurrentTurn();
        if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
        {
            throw new InvalidOperationException("Player must perform at least one action before ending turn");
        }

        await _turnOrchestrator.EndTurnAsync(session);
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> MakeAiTurnAsync(GameSession session)
    {
        throw new ArgumentException("AI turns are not supported in LocalCoop mode");
    }

    public override async Task<GameSessionDto> MovePieceAsync(GameSession session, string pieceId, PositionDto targetPosition)
    {
        var dto = new ExecuteActionDto
        {
            PieceId = pieceId,
            Type = "Move",
            TargetPosition = targetPosition
        };
        return await ExecuteActionAsync(session, dto);
    }

    public override async Task<List<PositionDto>> GetAvailableActionsAsync(GameSession session, string pieceId, string actionType, string? abilityName = null)
    {
        var actions = await _actionQueryService.GetAvailableActionsAsync(session, pieceId, actionType, abilityName);
        return actions.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList();
    }

    public override async Task<GameSessionDto> ExecuteAbilityAsync(GameSession session, AbilityRequestDto request)
    {
        var dto = new ExecuteActionDto
        {
            PieceId = request.PieceId,
            Type = "Ability",
            TargetPosition = request.Target,
            Description = request.AbilityName
        };
        return await ExecuteActionAsync(session, dto);
    }

    public override async Task<GameSessionDto> ExecuteEvolutionAsync(GameSession session, string pieceId, string targetType)
    {
        var dto = new ExecuteActionDto
        {
            PieceId = pieceId,
            Type = "Evolve",
            Description = targetType
        };
        return await ExecuteActionAsync(session, dto);
    }

    public override async Task<GameSessionDto> GetSessionAsync(Guid sessionId)
    {
        var session = await _sessionManagementService.GetSessionAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException("Game session not found");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> StartGameAsync(GameSession session)
    {
        await _sessionManagementService.StartGameAsync(session);
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> CompleteGameAsync(GameSession session, GameResult result)
    {
        await _sessionManagementService.CompleteGameAsync(session, result);
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> TutorialTransitionAsync(GameSession session, TutorialTransitionRequestDto request)
    {
        throw new NotImplementedException("Tutorial transitions are not supported in Local mode");
    }
}
