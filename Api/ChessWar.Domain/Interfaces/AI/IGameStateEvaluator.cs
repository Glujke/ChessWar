using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Интерфейс для оценки состояния игры
/// </summary>
public interface IGameStateEvaluator
{
    /// <summary>
    /// Оценить текущее состояние игры для игрока
    /// </summary>
    /// <param name="session">Игровая сессия</param>
    /// <param name="player">Игрок для которого оцениваем</param>
    /// <returns>Оценка от -100 (очень плохо) до +100 (очень хорошо)</returns>
    double EvaluateGameState(GameSession session, Player player);
    
    /// <summary>
    /// Оценить позицию фигуры на доске
    /// </summary>
    double EvaluatePiecePosition(Piece piece, GameSession session);
    
    /// <summary>
    /// Оценить угрозу для короля
    /// </summary>
    double EvaluateKingThreat(GameSession session, Player player);
    
    /// <summary>
    /// Оценить материальное преимущество
    /// </summary>
    double EvaluateMaterialAdvantage(GameSession session, Player player);
}
