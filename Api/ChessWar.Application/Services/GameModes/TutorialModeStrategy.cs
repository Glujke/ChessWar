using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.GameModes;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Services.Common;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using AutoMapper;

namespace ChessWar.Application.Services.GameModes;

/// <summary>
/// Стратегия для Tutorial режима (полная реализация)
/// </summary>
public class TutorialModeStrategy : BaseGameModeStrategy
{
    private readonly IBattlePresetService _battlePresetService;

    public TutorialModeStrategy(
        IGameSessionManagementService sessionManagementService,
        IActionExecutionService actionExecutionService,
        ITurnOrchestrator turnOrchestrator,
        IAITurnService aiTurnService,
        IActionQueryService actionQueryService,
        ITutorialService tutorialService,
        IBattlePresetService battlePresetService,
        IMapper mapper)
        : base(sessionManagementService, actionExecutionService, turnOrchestrator, aiTurnService, actionQueryService, tutorialService, mapper)
    {
        _battlePresetService = battlePresetService ?? throw new ArgumentNullException(nameof(battlePresetService));
    }

    public override async Task<GameSessionDto> CreateSessionAsync(CreateGameSessionDto dto)
    {
        var tutorialDto = new CreateGameSessionDto
        {
            Player1Name = dto.Player1Name,
            Player2Name = "AI",
            Mode = "Tutorial",
            TutorialSessionId = dto.TutorialSessionId
        };

        var session = await _sessionManagementService.CreateGameSessionAsync(tutorialDto);
        await _sessionManagementService.StartGameAsync(session);
        await _battlePresetService.ApplyPresetAsync(session, "Battle1");
        
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> ExecuteActionAsync(GameSession session, ExecuteActionDto dto)
    {
        var piece = session.Board?.Pieces?.FirstOrDefault(p => p.Id == int.Parse(dto.PieceId));
        if (piece == null)
        {
            throw new InvalidOperationException("Piece not found");
        }

        if (session.CurrentTurn?.ActiveParticipant?.Pieces?.Contains(piece) != true)
        {
            throw new InvalidOperationException("Cannot select piece that doesn't belong to active participant");
        }

        var success = await _actionExecutionService.ExecuteActionAsync(session, dto);
        if (!success)
        {
            throw new InvalidOperationException("Tutorial action execution failed");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> EndTurnAsync(GameSession session)
    {
        await _turnOrchestrator.EndTurnAsync(session);
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> MakeAiTurnAsync(GameSession session)
    {
        if (session.Status != ChessWar.Domain.Enums.GameStatus.Active)
        {
            return _mapper.Map<GameSessionDto>(session);
        }
        
        var success = await _aiTurnService.MakeAiTurnAsync(session);
        if (!success)
        {
            if (session.Status == ChessWar.Domain.Enums.GameStatus.Active)
            {
                var currentTurn = session.GetCurrentTurn();
                if (currentTurn.Actions == null || currentTurn.Actions.Count == 0)
                {
                    currentTurn.AddAction(new ChessWar.Domain.ValueObjects.TurnAction("Pass", string.Empty, null));
                }
                await _turnOrchestrator.EndTurnAsync(session);
            }
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> ExecuteAbilityAsync(GameSession session, AbilityRequestDto request)
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

    public override async Task<GameSessionDto> ExecuteEvolutionAsync(GameSession session, string pieceId, string targetType)
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
            throw new InvalidOperationException("Evolution execution failed");
        }
        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> CompleteGameAsync(GameSession session, GameResult result)
    {
        await _sessionManagementService.CompleteGameAsync(session, result);
        
        if (session.TutorialSessionId.HasValue && result == GameResult.Player1Victory)
        {
            var enemy = session.GetPlayer2Pieces();
            var hasKnight = enemy.Any(p => p.Type == PieceType.Knight);
            var hasBishop = enemy.Any(p => p.Type == PieceType.Bishop);
            var nextStage = (hasKnight && hasBishop) ? "Boss" : "Battle2";

            var next = await _sessionManagementService.CreateGameSessionAsync(new CreateGameSessionDto
            {
                Player1Name = session.Player1.Name,
                Player2Name = session.Player2.Name,
                Mode = session.Mode,
                TutorialSessionId = session.TutorialSessionId
            });
            await _sessionManagementService.StartGameAsync(next);
            await _battlePresetService.ApplyPresetAsync(next, nextStage);

            return _mapper.Map<GameSessionDto>(next);
        }

        return _mapper.Map<GameSessionDto>(session);
    }

    public override async Task<GameSessionDto> TutorialTransitionAsync(GameSession session, TutorialTransitionRequestDto request)
    {
        if (string.Equals(request.Action, "replay", StringComparison.OrdinalIgnoreCase))
        {
            var newGame = await _sessionManagementService.CreateGameSessionAsync(new CreateGameSessionDto
            {
                Player1Name = "P1",
                Player2Name = "AI",
                Mode = "Tutorial"
            });
            await _sessionManagementService.StartGameAsync(newGame);
            await _battlePresetService.ApplyPresetAsync(newGame, "Battle1");

            return _mapper.Map<GameSessionDto>(newGame);
        }

        if (session.Result == null || session.Result != GameResult.Player1Victory)
        {
            throw new InvalidOperationException("Stage not completed");
        }

        var activeTutorialSessionId = request.TutorialSessionId ?? session.TutorialSessionId;
        
        string nextStage;
        
        if (activeTutorialSessionId.HasValue)
        {
            try
            {
                var tutorialSession = await _tutorialService.AdvanceToNextStageAsync(activeTutorialSessionId.Value);
                
                nextStage = tutorialSession.CurrentStage switch
                {
                    TutorialStage.Battle1 => "Battle2",
                    TutorialStage.Battle2 => "Boss", 
                    TutorialStage.Boss => "Completed",
                    TutorialStage.Completed => "Completed",
                    _ => "Battle2" 
                };
                
                if (tutorialSession.IsCompleted || nextStage == "Completed")
                {
                    throw new InvalidOperationException("Tutorial is completed - no more stages available");
                }
            }
            catch (Exception)
            {
                var enemy = session.GetPlayer2Pieces();
                var hasKnight = enemy.Any(p => p.Type == PieceType.Knight);
                var hasBishop = enemy.Any(p => p.Type == PieceType.Bishop);
                nextStage = (hasKnight && hasBishop) ? "Boss" : "Battle2";
            }
        }
        else
        {
            var enemy = session.GetPlayer2Pieces();
            var hasKnight = enemy.Any(p => p.Type == PieceType.Knight);
            var hasBishop = enemy.Any(p => p.Type == PieceType.Bishop);
            nextStage = (hasKnight && hasBishop) ? "Boss" : "Battle2";
        }
        
        var next = await _sessionManagementService.CreateGameSessionAsync(new CreateGameSessionDto
        {
            Player1Name = session.Player1.Name,
            Player2Name = session.Player2.Name,
            Mode = session.Mode,
            TutorialSessionId = activeTutorialSessionId
        });
        await _sessionManagementService.StartGameAsync(next);
        await _battlePresetService.ApplyPresetAsync(next, nextStage);

        return _mapper.Map<GameSessionDto>(next);
    }

}
