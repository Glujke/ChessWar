namespace ChessWar.Domain.Interfaces.Configuration;

/// <summary>
/// Интерфейс для генерации уникальных ID фигур
/// </summary>
public interface IPieceIdGenerator
{
    /// <summary>
    /// Генерирует следующий уникальный ID для фигуры
    /// </summary>
    int GetNextId();
}
