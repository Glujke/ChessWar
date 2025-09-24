using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Services.GameManagement;

/// <summary>
/// Сервис управления игрой
/// </summary>
public class GameManagementService : IGameManagementService
{
    public void StartGame(GameSession gameSession)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        gameSession.StartGame();
    }

    public void CompleteGame(GameSession gameSession, GameResult result)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        gameSession.CompleteGame(result);
    }
}
