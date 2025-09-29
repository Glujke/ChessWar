using ChessWar.Application.Interfaces.Board;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Services.Common;

/// <summary>
/// Сервис для получения контекста доски и фигур
/// </summary>
public interface IBoardContextService
{
    /// <summary>
    /// Получает все живые фигуры с доски
    /// </summary>
    /// <param name="gameSession">Игровая сессия</param>
    /// <returns>Список живых фигур</returns>
    Task<IEnumerable<Piece>> GetAlivePiecesAsync(GameSession gameSession, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает фигуру по ID из игровой сессии
    /// </summary>
    /// <param name="gameSession">Игровая сессия</param>
    /// <param name="pieceId">ID фигуры</param>
    /// <returns>Фигура или null, если не найдена</returns>
    Piece? GetPieceById(GameSession gameSession, int pieceId);

    /// <summary>
    /// Получает фигуру по позиции
    /// </summary>
    /// <param name="gameSession">Игровая сессия</param>
    /// <param name="position">Позиция</param>
    /// <returns>Фигура или null, если позиция пуста</returns>
    Piece? GetPieceAtPosition(GameSession gameSession, Position position);
}

/// <summary>
/// Реализация сервиса контекста доски
/// </summary>
public class BoardContextService : IBoardContextService
{
    private readonly IBoardService _boardService;

    public BoardContextService(IBoardService boardService)
    {
        _boardService = boardService ?? throw new ArgumentNullException(nameof(boardService));
    }

    public Task<IEnumerable<Piece>> GetAlivePiecesAsync(GameSession gameSession, CancellationToken cancellationToken = default)
    {
        if (gameSession?.Board?.Pieces == null)
        {
            return Task.FromResult(Enumerable.Empty<Piece>());
        }

        return Task.FromResult(gameSession.Board.Pieces.Where(p => p.IsAlive));
    }

    public Piece? GetPieceById(GameSession gameSession, int pieceId)
    {
        if (gameSession?.Board?.Pieces == null)
        {
            return null;
        }

        return gameSession.Board.Pieces.FirstOrDefault(p => p.Id == pieceId);
    }

    public Piece? GetPieceAtPosition(GameSession gameSession, Position position)
    {
        if (gameSession?.Board?.Pieces == null)
        {
            return null;
        }

        return gameSession.Board.Pieces.FirstOrDefault(p => p.Position.Equals(position));
    }
}
