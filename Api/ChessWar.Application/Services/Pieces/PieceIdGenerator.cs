using ChessWar.Domain.Interfaces.Configuration;

namespace ChessWar.Application.Services.Pieces;

/// <summary>
/// Сервис для генерации уникальных ID фигур
/// </summary>
public class PieceIdGenerator : IPieceIdGenerator
{
    private static int _pieceIdSeed = 0;

    /// <summary>
    /// Генерирует следующий уникальный ID для фигуры
    /// </summary>
    public int GetNextId()
    {
        return System.Threading.Interlocked.Increment(ref _pieceIdSeed);
    }
}
