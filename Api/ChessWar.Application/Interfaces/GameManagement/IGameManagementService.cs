using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Interfaces.GameManagement;

/// <summary>
/// Сервис управления игрой
/// </summary>
public interface IGameManagementService
{
    /// <summary>
    /// Начинает игру
    /// </summary>
    void StartGame(GameSession gameSession);
    
    /// <summary>
    /// Завершает игру
    /// </summary>
    void CompleteGame(GameSession gameSession, GameResult result);
}
