using AutoMapper;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Interfaces.Tutorial;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace ChessWar.Api.Controllers;

/// <summary>
/// Контроллер для управления игровыми сессиями
/// </summary>
public class GameSessionController : BaseController
{
    private readonly IGameSessionManagementService _sessionManagementService;
    private readonly ITurnExecutionService _turnExecutionService;
    private readonly IBattlePresetService _battlePresetService;
    private readonly ITutorialService _tutorialService;
    private readonly IMapper _mapper;

    public GameSessionController(
        IGameSessionManagementService sessionManagementService,
        ITurnExecutionService turnExecutionService,
        IBattlePresetService battlePresetService,
        ITutorialService tutorialService,
        ILogger<GameSessionController> logger,
        IMapper mapper) : base(logger)
    {
        _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
        _turnExecutionService = turnExecutionService ?? throw new ArgumentNullException(nameof(turnExecutionService));
        _battlePresetService = battlePresetService ?? throw new ArgumentNullException(nameof(battlePresetService));
        _tutorialService = tutorialService ?? throw new ArgumentNullException(nameof(tutorialService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Создаёт новую игровую сессию
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<GameSessionDto>> CreateGameSession([FromBody] CreateGameSessionDto dto)
    {
        var gameSession = await _sessionManagementService.CreateGameSessionAsync(dto);
        await _sessionManagementService.StartGameAsync(gameSession);
        var result = _mapper.Map<GameSessionDto>(gameSession);

        LogInformation("Created game session {GameSessionId} with players {Player1} and {Player2}",
            gameSession.Id, dto.Player1Name, dto.Player2Name);

        return Ok(result);
    }

    /// <summary>
    /// Начинает игру
    /// </summary>
    [HttpPost("{gameSessionId}/start")]
    public ActionResult StartGame(Guid gameSessionId)
    {
        try
        {
            LogInformation("Started game session {GameSessionId}", gameSessionId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            LogWarning("Failed to start game session {GameSessionId}: {Error}", gameSessionId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            LogError(ex, "Unexpected error starting game session {GameSessionId}", gameSessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Завершает игру
    /// </summary>
    [HttpPost("{gameSessionId}/complete")]
    public async Task<ActionResult> CompleteGame(Guid gameSessionId, [FromBody] string? result, [FromQuery(Name = "result")] string? resultQuery)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }

        var effectiveResult = result ?? resultQuery;
        if (string.IsNullOrWhiteSpace(effectiveResult) || !Enum.TryParse<GameResult>(effectiveResult, ignoreCase: true, out var parsed))
        {
            return BadRequest("Invalid game result value");
        }

        await _sessionManagementService.CompleteGameAsync(session, parsed);
        LogInformation("Completed game session {GameSessionId} with result {Result}", gameSessionId, effectiveResult);

        if (!string.IsNullOrEmpty(resultQuery))
        {
            if (session.TutorialSessionId.HasValue)
            {
                if (parsed != GameResult.Player1Victory)
                {
                    return Problem(statusCode: 409, title: "StageNotCompleted");
                }

                var enemy = session.GetPlayer2Pieces();
                var hasKnight = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Knight);
                var hasBishop = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Bishop);
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

                var okQuery = new Dictionary<string, object?>
                {
                    ["gameSessionId"] = next.Id.ToString()
                };
                return new JsonResult(okQuery);
            }

            return parsed == GameResult.Player1Victory ? Ok() : Problem(statusCode: 409, title: "StageNotCompleted");
        }

        if (session.TutorialSessionId.HasValue && parsed == GameResult.Player1Victory)
        {
            var enemy = session.GetPlayer2Pieces();
            var hasKnight = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Knight);
            var hasBishop = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Bishop);
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

            var ok = new Dictionary<string, object?>
            {
                ["gameSessionId"] = next.Id.ToString()
            };
            return new JsonResult(ok);
        }

        return Ok();
    }

    /// <summary>
    /// Выполняет действие в ходе
    /// </summary>
    [HttpPost("{gameSessionId}/turn/action")]
    public async Task<ActionResult> ExecuteAction(Guid gameSessionId, [FromBody] ExecuteActionDto dto)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }
        var success = await _turnExecutionService.ExecuteActionAsync(session, dto);
        LogInformation("Executed action {ActionType} for piece {PieceId} in game session {GameSessionId}",
            dto.Type, dto.PieceId, gameSessionId);

        if (!success)
        {
            return Problem(statusCode: 400, title: "RuleViolation", detail: "Action failed due to rule validation");
        }

        var sessionDto = _mapper.Map<GameSessionDto>(session);
        return Ok(sessionDto);
    }

    /// <summary>
    /// Использует способность в текущем ходе
    /// </summary>
    [HttpPost("{gameSessionId}/turn/ability")]
    public async Task<ActionResult> UseAbility(Guid gameSessionId, [FromBody] AbilityRequestDto request)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }

        var dto = new ExecuteActionDto
        {
            Type = "Ability",
            PieceId = request.PieceId,
            TargetPosition = request.Target,
            Description = request.AbilityName
        };

        var ok = await _turnExecutionService.ExecuteActionAsync(session, dto);
        if (!ok)
        {
            return Problem(statusCode: 400, title: "RuleViolation", detail: "Ability failed due to rule validation");
        }
        LogInformation("Executed ability {Ability} for piece {PieceId} in game session {GameSessionId}", request.AbilityName, request.PieceId, gameSessionId);
        var sessionDto = _mapper.Map<GameSessionDto>(session);
        return Ok(sessionDto);
    }

    /// <summary>
    /// Эволюционировать фигуру
    /// </summary>
    [HttpPost("{gameSessionId}/evolve")]
    public async Task<ActionResult> Evolve(Guid gameSessionId, [FromBody] EvolveRequestDto request)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }

        var dto = new ExecuteActionDto
        {
            Type = "Evolve",
            PieceId = request.PieceId,
            TargetPosition = null,
            Description = request.TargetType
        };

        var ok = await _turnExecutionService.ExecuteActionAsync(session, dto);
        if (!ok)
        {
            return Problem(statusCode: 400, title: "RuleViolation", detail: "Evolution failed due to rule validation");
        }
        var sessionDto = _mapper.Map<GameSessionDto>(session);
        return Ok(sessionDto);
    }

    /// <summary>
    /// Завершает текущий ход
    /// </summary>
    [HttpPost("{gameSessionId}/turn/end")]
    public async Task<ActionResult> EndTurn(Guid gameSessionId)
    {
        LogInformation("Завершение хода для сессии {GameSessionId}", gameSessionId);

        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            LogInformation("Сессия не найдена: {GameSessionId}", gameSessionId);
            return NotFound();
        }

        try
        {
            await _turnExecutionService.EndTurnAsync(session);
            LogInformation("Ход завершён для сессии {GameSessionId}", gameSessionId);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("хотя бы одного действия"))
        {
            LogWarning("Валидация хода не пройдена для {GameSessionId}: {Error}", gameSessionId, ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при завершении хода для {GameSessionId}: {Error}", gameSessionId, ex.Message);
            throw;
        }

        return Ok();
    }


    /// <summary>
    /// Переместить фигуру
    /// </summary>
    [HttpPost("{gameSessionId}/move")]
    public async Task<ActionResult> MovePiece(Guid gameSessionId, [FromBody] MovePieceRequest request)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }

        try
        {
            var pieceId = int.Parse(request.PieceId);
            var targetPosition = new Position(request.TargetPosition.X, request.TargetPosition.Y);

            var success = await _turnExecutionService.ExecuteMoveAsync(session, pieceId, targetPosition);
            if (!success)
            {
                return BadRequest("Invalid move or not enough mana");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            LogError(ex, "Error moving piece in session {GameSessionId}: {Error}", gameSessionId, ex.Message);
            return BadRequest("Error moving piece");
        }
    }

    public class MovePieceRequest
    {
        public string PieceId { get; set; } = string.Empty;
        public PositionDto TargetPosition { get; set; } = new();
    }

    /// <summary>
    /// Получает доступные действия для фигуры
    /// </summary>
    [HttpGet("{gameSessionId}/piece/{pieceId}/actions")]
    public async Task<ActionResult<List<PositionDto>>> GetAvailableActions(
        Guid gameSessionId,
        string pieceId,
        [FromQuery] string actionType,
        [FromQuery(Name = "ability")] string? abilityName = null)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }

        var actions = await _turnExecutionService.GetAvailableActionsAsync(session, pieceId, actionType, abilityName);
        LogInformation("Retrieved available actions for piece {PieceId} in game session {GameSessionId}",
            pieceId, gameSessionId);

        return Ok(actions);
    }

    /// <summary>
    /// Переход в обучении: следующий этап или повтор
    /// </summary>
    public sealed class TutorialTransitionRequest
    {
        public string? action { get; set; }
    }

    [HttpPost("{gameSessionId}/tutorial/transition")]
    public async Task<IActionResult> TutorialTransition(Guid gameSessionId, [FromBody] TutorialTransitionRequest body, [FromQuery] string? embed = null, [FromQuery] Guid? tutorialSessionId = null)
    {
        try
        {
            var action = body?.action ?? string.Empty;
            if (string.Equals(action, "replay", StringComparison.OrdinalIgnoreCase))
            {
                var newGame = await _sessionManagementService.CreateGameSessionAsync(new CreateGameSessionDto
                {
                    Player1Name = "P1",
                    Player2Name = "AI",
                    Mode = "AI"
                });
                await _sessionManagementService.StartGameAsync(newGame);

                var response = new Dictionary<string, object?>
                {
                    ["gameSessionId"] = newGame.Id.ToString()
                };
                if (string.Equals(embed, "(game)", StringComparison.OrdinalIgnoreCase))
                {
                    response["_embedded"] = new Dictionary<string, object?>
                    {
                        ["game"] = _mapper.Map<GameSessionDto>(newGame)
                    };
                }
                return new JsonResult(response);
            }

            var current = await _sessionManagementService.GetSessionAsync(gameSessionId);
            if (current == null)
            {
                return NotFound();
            }
            if (current.Status != ChessWar.Domain.Enums.GameStatus.Finished || current.Result != ChessWar.Domain.Enums.GameResult.Player1Victory)
            {
                return Problem(statusCode: 409, title: "StageNotCompleted");
            }

            var activeTutorialSessionId = tutorialSessionId ?? current.TutorialSessionId;

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
                        return Problem(statusCode: 409, title: "TutorialCompleted", detail: "Tutorial is completed - no more stages available");
                    }
                }
                catch (Exception ex)
                {
                    var safeTutorialId = current.TutorialSessionId ?? Guid.Empty;
                    LogError(ex, "Failed to advance tutorial session {TutorialSessionId}, using fallback logic", safeTutorialId);
                    var enemy = current.GetPlayer2Pieces();
                    var hasKnight = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Knight);
                    var hasBishop = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Bishop);
                    nextStage = (hasKnight && hasBishop) ? "Boss" : "Battle2";
                }
            }
            else
            {
                var enemy = current.GetPlayer2Pieces();
                var hasKnight = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Knight);
                var hasBishop = enemy.Any(p => p.Type == ChessWar.Domain.Enums.PieceType.Bishop);
                nextStage = (hasKnight && hasBishop) ? "Boss" : "Battle2";
            }

            var next = await _sessionManagementService.CreateGameSessionAsync(new CreateGameSessionDto
            {
                Player1Name = current.Player1.Name,
                Player2Name = current.Player2.Name,
                Mode = current.Mode,
                TutorialSessionId = activeTutorialSessionId
            });
            await _sessionManagementService.StartGameAsync(next);
            await _battlePresetService.ApplyPresetAsync(next, nextStage);

            var ok = new Dictionary<string, object?>
            {
                ["gameSessionId"] = next.Id.ToString()
            };
            if (string.Equals(embed, "(game)", StringComparison.OrdinalIgnoreCase))
            {
                ok["_embedded"] = new Dictionary<string, object?>
                {
                    ["game"] = _mapper.Map<GameSessionDto>(next)
                };
            }
            return new JsonResult(ok);
        }
        catch (Exception ex)
        {
            LogError(ex, "Tutorial transition failed for game {GameSessionId}", gameSessionId);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Получить состояние игровой сессии
    /// </summary>
    [HttpGet("{gameSessionId}")]
    public async Task<ActionResult<GameSessionDto>> GetSession(Guid gameSessionId)
    {
        var session = await _sessionManagementService.GetSessionAsync(gameSessionId);
        if (session == null)
        {
            return NotFound();
        }
        var dto = _mapper.Map<GameSessionDto>(session);
        return Ok(dto);
    }

}
