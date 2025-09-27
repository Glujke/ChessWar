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
/// Стратегия для Online режима (заглушка - в разработке)
/// </summary>
public class OnlineModeStrategy : BaseGameModeStrategy
{
    public OnlineModeStrategy(
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
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> ExecuteActionAsync(GameSession session, ExecuteActionDto dto)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> EndTurnAsync(GameSession session)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> MakeAiTurnAsync(GameSession session)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> MovePieceAsync(GameSession session, string pieceId, PositionDto targetPosition)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<List<PositionDto>> GetAvailableActionsAsync(GameSession session, string pieceId, string actionType, string? abilityName = null)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> ExecuteAbilityAsync(GameSession session, AbilityRequestDto request)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> ExecuteEvolutionAsync(GameSession session, string pieceId, string targetType)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> GetSessionAsync(Guid sessionId)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> StartGameAsync(GameSession session)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }

    public override async Task<GameSessionDto> CompleteGameAsync(GameSession session, GameResult result)
    {
        throw new NotImplementedException("Online режим находится в разработке. Используйте Tutorial режим для демонстрации.");
    }
}
