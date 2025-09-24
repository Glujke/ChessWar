using AutoMapper;
using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Entities;

namespace ChessWar.Application.Services.Configuration;

/// <summary>
/// Сервис для управления режимами игры
/// </summary>
public class GameModeService : IGameModeService
{
    private readonly IMapper _mapper;
    private readonly IGameSessionRepository _gameSessionRepository;
    private readonly IGameModeRepository _gameModeRepository;
    private readonly IGameHubClient _gameHubClient;

    public GameModeService(IMapper mapper, IGameSessionRepository gameSessionRepository, IGameModeRepository gameModeRepository, IGameHubClient gameHubClient)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _gameSessionRepository = gameSessionRepository ?? throw new ArgumentNullException(nameof(gameSessionRepository));
        _gameModeRepository = gameModeRepository ?? throw new ArgumentNullException(nameof(gameModeRepository));
        _gameHubClient = gameHubClient ?? throw new ArgumentNullException(nameof(gameHubClient));
    }

    public async Task<TutorialSessionDto> StartTutorialAsync(CreateTutorialSessionDto dto, CancellationToken cancellationToken = default)
    {
        var player = new Player(dto.PlayerId ?? "Tutorial Player", Team.Elves);
        var tutorialSession = new TutorialSession(player, showHints: true);
        
        await _gameModeRepository.SaveModeAsync(tutorialSession, cancellationToken);
        
        await _gameHubClient.SendToGroupAsync(
            tutorialSession.Id.ToString(), 
            "SessionCreated", 
            new { SessionId = tutorialSession.Id, Mode = "Tutorial" }, 
            cancellationToken);
        
        var result = _mapper.Map<TutorialSessionDto>(tutorialSession);
        result.Id = tutorialSession.Id; 
        result.Mode = "Tutorial";
        result.SignalRUrl = "/gameHub";
        result.CurrentStage = tutorialSession.CurrentStage;
        result.Progress = tutorialSession.Progress;
        result.IsCompleted = tutorialSession.IsCompleted;
        result.Board = new TutorialBoardDto { Width = Domain.Entities.GameBoard.Size, Height = Domain.Entities.GameBoard.Size };
        result.Pieces = new List<PieceDto>();
        result.Scenario = new TutorialScenarioDto { Type = "Battle", Difficulty = "Easy" };
        
        return result;
    }

    public async Task<AiSessionDto> StartAiGameAsync(CreateAiSessionDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("AI sessions are not implemented yet");
    }

    public async Task<GameSessionDto> StartLocalGameAsync(CreateLocalSessionDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Local sessions are not implemented yet");
    }

    public async Task<OnlineSessionDto> StartOnlineGameAsync(CreateOnlineSessionDto dto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Online sessions are not implemented yet");
    }

    public async Task<object> GetAvailableModesAsync(CancellationToken cancellationToken = default)
    {
        var modes = new
        {
            Tutorial = new { Name = "Обучение", Description = "Изучите основы игры" },
            AI = new { Name = "Игра с ИИ", Description = "Сражайтесь с искусственным интеллектом" },
            Local = new { Name = "Локальная игра", Description = "Игра на одном устройстве" },
            Online = new { Name = "Сетевая игра", Description = "Игра через интернет" },
            Statistics = new { Name = "Статистика", Description = "Просмотр статистики игрока" }
        };

        await Task.Delay(1, cancellationToken);
        return modes;
    }

    public async Task<PlayerStatsDto> GetPlayerStatsAsync(string playerId, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Player stats are not implemented yet");
    }
}