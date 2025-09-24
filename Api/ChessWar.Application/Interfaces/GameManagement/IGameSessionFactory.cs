using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Interfaces.GameManagement;

/// <summary>
/// Фабрика для создания игровых сессий
/// </summary>
public interface IGameSessionFactory
{
    /// <summary>
    /// Создаёт новую игровую сессию
    /// </summary>
    GameSession CreateGameSession(CreateGameSessionDto dto);
    
    /// <summary>
    /// Создаёт игрока с начальными фигурами
    /// </summary>
    Player CreatePlayerWithInitialPieces(string name, Team team);
}
