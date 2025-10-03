using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Services.Common;

/// <summary>
/// Сервис для валидации фигур и их состояний
/// </summary>
public interface IPieceValidationService
{
    /// <summary>
    /// Проверяет, что фигура существует и жива
    /// </summary>
    /// <param name="piece">Фигура для проверки</param>
    /// <param name="pieceId">ID фигуры для сообщений об ошибках</param>
    /// <returns>True, если фигура валидна</returns>
    bool IsPieceValid(Piece? piece, int pieceId);

    /// <summary>
    /// Проверяет, что фигура существует и жива, выбрасывает исключение если нет
    /// </summary>
    /// <param name="piece">Фигура для проверки</param>
    /// <param name="pieceId">ID фигуры для сообщений об ошибках</param>
    /// <exception cref="ArgumentException">Если фигура не найдена или мертва</exception>
    void ValidatePiece(Piece? piece, int pieceId);

    /// <summary>
    /// Проверяет, что позиция находится в пределах доски
    /// </summary>
    /// <param name="position">Позиция для проверки</param>
    /// <returns>True, если позиция валидна</returns>
    bool IsPositionValid(Position position);

    /// <summary>
    /// Проверяет, что позиция находится в пределах доски, выбрасывает исключение если нет
    /// </summary>
    /// <param name="position">Позиция для проверки</param>
    /// <exception cref="ArgumentException">Если позиция вне доски</exception>
    void ValidatePosition(Position position);
}

/// <summary>
/// Реализация сервиса валидации фигур
/// </summary>
public class PieceValidationService : IPieceValidationService
{
    private const int BoardSize = 8;

    public bool IsPieceValid(Piece? piece, int pieceId)
    {
        return piece != null && piece.IsAlive;
    }

    public void ValidatePiece(Piece? piece, int pieceId)
    {
        if (piece == null)
        {
            throw new ArgumentException($"Piece with ID {pieceId} not found", nameof(piece));
        }

        if (!piece.IsAlive)
        {
            throw new ArgumentException($"Piece with ID {pieceId} is not alive", nameof(piece));
        }
    }

    public bool IsPositionValid(Position position)
    {
        return position.X >= 0 && position.X < BoardSize &&
               position.Y >= 0 && position.Y < BoardSize;
    }

    public void ValidatePosition(Position position)
    {
        if (!IsPositionValid(position))
        {
            throw new ArgumentException($"Position ({position.X}, {position.Y}) is outside board boundaries. Board size: {BoardSize}x{BoardSize}", nameof(position));
        }
    }
}

