using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Domain.Interfaces.AI;

/// <summary>
/// Интерфейс для сессии с ИИ
/// </summary>
public interface IAiMode : IGameModeBase
{
    /// <summary>
    /// Сложность ИИ
    /// </summary>
    AiDifficulty Difficulty { get; }
    
    /// <summary>
    /// ID игрока
    /// </summary>
    string PlayerId { get; }
}

