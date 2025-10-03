using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.DataAccess;

namespace ChessWar.Application.Services.GameManagement;

/// <summary>
/// Сервис управления игровыми сессиями
/// </summary>
public class GameSessionManagementService : IGameSessionManagementService
{
    private readonly IPlayerManagementService _playerManagementService;
    private readonly IGameSessionRepository _sessionRepository;

    public GameSessionManagementService(
        IPlayerManagementService playerManagementService,
        IGameSessionRepository sessionRepository)
    {
        _playerManagementService = playerManagementService ?? throw new ArgumentNullException(nameof(playerManagementService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<GameSession> CreateGameSessionAsync(CreateGameSessionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Player1Name))
            throw new ArgumentException("Player1 name cannot be empty", nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.Player2Name))
            throw new ArgumentException("Player2 name cannot be empty", nameof(dto));

        var mode = (dto.Mode ?? string.Empty).Trim();
        if (!string.Equals(mode, "AI", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(mode, "LocalCoop", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(mode, "Tutorial", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid session mode. Allowed values: 'AI', 'LocalCoop', or 'Tutorial'", nameof(dto.Mode));
        }

        var player1 = _playerManagementService.CreatePlayerWithInitialPieces(dto.Player1Name, Team.Elves);

        Participant player2;
        if (string.Equals(mode, "AI", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(mode, "Tutorial", StringComparison.OrdinalIgnoreCase))
        {
            player2 = _playerManagementService.CreateAIWithInitialPieces(Team.Orcs);
        }
        else
        {
            player2 = _playerManagementService.CreatePlayerWithInitialPieces(dto.Player2Name, Team.Orcs);
        }

        var gameSession = new GameSession(player1, player2, mode);


        if (dto.TutorialSessionId.HasValue)
        {
            gameSession.SetTutorialSessionId(dto.TutorialSessionId.Value);
        }

        await _sessionRepository.SaveAsync(gameSession, cancellationToken);

        return gameSession;
    }

    public async Task StartGameAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        gameSession.StartGame();
        await _sessionRepository.SaveAsync(gameSession, cancellationToken);
    }

    public async Task CompleteGameAsync(GameSession gameSession, GameResult result, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        gameSession.CompleteGame(result);
        await _sessionRepository.SaveAsync(gameSession, cancellationToken);
    }

    public async Task<GameSession?> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
    }
}
