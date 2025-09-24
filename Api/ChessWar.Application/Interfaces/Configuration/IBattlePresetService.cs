using ChessWar.Domain.Entities;

namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Применяет пресеты Battle1/Battle2/Boss к игровой сессии
/// </summary>
public interface IBattlePresetService
{
    /// <summary>
    /// Применить пресет стадии к игровой сессии (раскладка, сложность и т.д.)
    /// </summary>
    /// <param name="session">Игровая сессия</param>
    /// <param name="stage">Battle1|Battle2|Boss</param>
    Task ApplyPresetAsync(GameSession session, string stage, CancellationToken cancellationToken = default);
}


