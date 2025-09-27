using Microsoft.AspNetCore.Mvc;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Interfaces.GameManagement;
using AutoMapper;

namespace ChessWar.Api.Controllers;

/// <summary>
/// Контроллер для управления режимами игры - главная точка входа в приложение
/// </summary>
[ApiController]
[Route("api/v1/game")]
public class GameModeController : BaseController
{
    private readonly IGameModeService _gameModeService;
    private readonly IGameSessionManagementService _sessionManagementService;
    private readonly IBattlePresetService _battlePresetService;
    private readonly IMapper _mapper;

    public GameModeController(
        IGameModeService gameModeService,
        IGameSessionManagementService sessionManagementService,
        IBattlePresetService battlePresetService,
        IMapper mapper,
        ILogger<GameModeController> logger) : base(logger)
    {
        _gameModeService = gameModeService ?? throw new ArgumentNullException(nameof(gameModeService));
        _sessionManagementService = sessionManagementService ?? throw new ArgumentNullException(nameof(sessionManagementService));
        _battlePresetService = battlePresetService ?? throw new ArgumentNullException(nameof(battlePresetService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// 1. Начать обучение (демо версия)
    /// </summary>
    [HttpPost("tutorial")]
    public async Task<IActionResult> StartTutorial([FromBody] CreateTutorialSessionDto dto, [FromQuery] string? embed = null)
    {
        LogInformation("Starting tutorial session for player {PlayerId}", dto.PlayerId);

        var session = await _gameModeService.StartTutorialAsync(dto);

        var game = await _sessionManagementService.CreateGameSessionAsync(new CreateGameSessionDto
        {
            Player1Name = string.IsNullOrWhiteSpace(dto.PlayerId) ? "P1" : dto.PlayerId!,
            Player2Name = "AI",
            Mode = "Tutorial",
            TutorialSessionId = session.SessionId 
        });
        await _sessionManagementService.StartGameAsync(game);
        await _battlePresetService.ApplyPresetAsync(game, "Battle1");
        var gameSessionId = game.Id;

        var tutorialId = session.SessionId;
        var location = $"/api/v1/game/tutorial/{tutorialId}";
        Response.Headers["Location"] = location;
        Response.Headers["Content-Location"] = location;

        var response = new Dictionary<string, object?>
        {
            ["gameSessionId"] = gameSessionId.ToString(),
            ["sessionId"] = session.SessionId,
            ["mode"] = session.Mode,
            ["status"] = session.Status,
            ["stage"] = session.Stage,
            ["progress"] = session.Progress,
            ["isCompleted"] = session.IsCompleted,
            ["showHints"] = session.ShowHints,
            ["createdAt"] = session.CreatedAt,
            ["updatedAt"] = session.UpdatedAt,
            ["signalRUrl"] = session.SignalRUrl,
            ["scenario"] = session.Scenario,
            ["board"] = session.Board,
            ["pieces"] = session.Pieces,
        };

        if (string.Equals(embed, "(game)", StringComparison.OrdinalIgnoreCase))
        {
            response["_embedded"] = new Dictionary<string, object?>
            {
                ["game"] = _mapper.Map<GameSessionDto>(game)
            };
        }

        return Ok(response);
    }

    /// <summary>
    /// 2. Начать сетевую игру
    /// </summary>
    [HttpPost("online")]
    public async Task<ActionResult<OnlineSessionDto>> StartOnlineGame([FromBody] CreateOnlineSessionDto dto)
    {
        LogInformation("Starting online session for host {HostPlayerId}", dto.HostPlayerId);
        
        var session = await _gameModeService.StartOnlineGameAsync(dto);
        return Ok(session);
    }

    /// <summary>
    /// 3. Начать локальную игру
    /// </summary>
    [HttpPost("local")]
    public async Task<ActionResult<GameSessionDto>> StartLocalGame([FromBody] CreateLocalSessionDto dto)
    {
        LogInformation("Starting local session for players {Player1} vs {Player2}", dto.Player1Name, dto.Player2Name);
        
        var session = await _gameModeService.StartLocalGameAsync(dto);
        return Ok(session);
    }

    /// <summary>
    /// 4. Начать игру с ИИ
    /// </summary>
    [HttpPost("ai")]
    public async Task<ActionResult<AiSessionDto>> StartAiGame([FromBody] CreateAiSessionDto dto)
    {
        LogInformation("Starting AI session for player {PlayerId} with difficulty {Difficulty}", dto.PlayerId, dto.Difficulty);
        
        var session = await _gameModeService.StartAiGameAsync(dto);
        return Ok(session);
    }

    /// <summary>
    /// 5. Получить статистику игрока
    /// </summary>
    [HttpGet("stats/{playerId}")]
    public async Task<ActionResult<PlayerStatsDto>> GetPlayerStats(string playerId)
    {
        LogInformation("Getting stats for player {PlayerId}", playerId);
        
        var stats = await _gameModeService.GetPlayerStatsAsync(playerId);
        return Ok(stats);
    }

    /// <summary>
    /// Получить доступные режимы игры
    /// </summary>
    [HttpGet("modes")]
    public async Task<ActionResult<object>> GetAvailableModes()
    {
        var modes = await _gameModeService.GetAvailableModesAsync();
        return Ok(modes);
    }
}

