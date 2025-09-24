using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;

namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Сервис управления игроками
/// </summary>
public interface IPlayerManagementService
{
    /// <summary>
    /// Создаёт игрока с начальными фигурами
    /// </summary>
    Player CreatePlayerWithInitialPieces(string name, Team team);
    
    /// <summary>
    /// Находит фигуру по ID в сессии
    /// </summary>
    Piece? FindPieceById(GameSession gameSession, string pieceId);
}
