using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Interfaces.Pieces;

/// <summary>
/// Сервис для управления фигурами на доске
/// </summary>
public interface IPieceService
{
    /// <summary>
    /// Создает новую фигуру
    /// </summary>
    Task<Piece> CreatePieceAsync(PieceType type, Team team, Position position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает фигуру по ID
    /// </summary>
    Task<Piece?> GetPieceByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает все фигуры
    /// </summary>
    Task<IReadOnlyList<Piece>> GetAllPiecesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает фигуры по команде
    /// </summary>
    Task<IReadOnlyList<Piece>> GetPiecesByTeamAsync(Team team, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает живые фигуры
    /// </summary>
    Task<IReadOnlyList<Piece>> GetAlivePiecesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает живые фигуры по команде
    /// </summary>
    Task<IReadOnlyList<Piece>> GetAlivePiecesByTeamAsync(Team team, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет позицию фигуры
    /// </summary>
    Task<Piece> UpdatePiecePositionAsync(int pieceId, Position newPosition, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет характеристики фигуры
    /// </summary>
    Task<Piece> UpdatePieceStatsAsync(int pieceId, int? hp = null, int? atk = null, int? mp = null, int? xp = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет фигуру
    /// </summary>
    Task DeletePieceAsync(int pieceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет, свободна ли позиция
    /// </summary>
    Task<bool> IsPositionFreeAsync(Position position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Проверяет, находится ли позиция в пределах доски
    /// </summary>
    bool IsPositionOnBoard(Position position);
}
