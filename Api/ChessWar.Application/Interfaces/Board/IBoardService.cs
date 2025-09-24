using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Interfaces.Board;

/// <summary>
/// Сервис для управления игровой доской
/// </summary>
public interface IBoardService
{
    /// <summary>
    /// Получает текущее состояние доски
    /// </summary>
    Task<GameBoard> GetBoardAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Сбрасывает доску (удаляет все фигуры)
    /// </summary>
    Task ResetBoardAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Настраивает начальную расстановку фигур
    /// </summary>
    Task SetupInitialPositionAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Размещает фигуру на доске
    /// </summary>
    Task<Piece> PlacePieceAsync(PieceType type, Team team, Position position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Перемещает фигуру на новую позицию
    /// </summary>
    Task<Piece> MovePieceAsync(int pieceId, Position newPosition, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает фигуру по позиции
    /// </summary>
    Task<Piece?> GetPieceAtPositionAsync(Position position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет, свободна ли позиция
    /// </summary>
    Task<bool> IsPositionFreeAsync(Position position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет, находится ли позиция в пределах доски
    /// </summary>
    bool IsPositionOnBoard(Position position);
    
    /// <summary>
    /// Получает размер доски
    /// </summary>
    int GetBoardSize();
}
