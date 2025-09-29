using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.DataAccess;

/// <summary>
/// Репозиторий для работы с фигурами на доске
/// </summary>
public interface IPieceRepository
{
    /// <summary>
    /// Получает все фигуры на доске
    /// </summary>
    Task<IReadOnlyList<Piece>> GetAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает фигуры по команде
    /// </summary>
    Task<IReadOnlyList<Piece>> GetByTeamAsync(Team team, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает живые фигуры
    /// </summary>
    Task<IReadOnlyList<Piece>> GetAlivePiecesAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает живые фигуры по команде
    /// </summary>
    Task<IReadOnlyList<Piece>> GetAlivePiecesByTeamAsync(Team team, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает фигуру по ID
    /// </summary>
    Task<Piece?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает фигуру по позиции
    /// </summary>
    Task<Piece?> GetByPositionAsync(Position position, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Добавляет фигуру
    /// </summary>
    Task AddAsync(Piece piece, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет фигуру
    /// </summary>
    Task UpdateAsync(Piece piece, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет фигуру
    /// </summary>
    Task DeleteAsync(Piece piece, CancellationToken cancellationToken = default);
}
