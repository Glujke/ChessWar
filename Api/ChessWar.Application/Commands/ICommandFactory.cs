using ChessWar.Application.DTOs;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Commands;

/// <summary>
/// Фабрика для создания команд
/// </summary>
public interface ICommandFactory
{
    /// <summary>
    /// Создаёт команду на основе типа действия
    /// </summary>
    ICommand? CreateCommand(string actionType, GameSession gameSession, Turn turn, Piece piece, PositionDto? targetPosition, string? description);
}
