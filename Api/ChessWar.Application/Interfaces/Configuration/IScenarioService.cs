using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Сервис для управления сценариями
/// </summary>
public interface IScenarioService
{
    /// <summary>
    /// Создает сценарий боя с ИИ (этап 1 и 2)
    /// </summary>
    Task<IScenario> CreateBattleScenarioAsync(AiDifficulty difficulty, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создает сценарий боя с боссом (этап 3)
    /// </summary>
    Task<IScenario> CreateBossScenarioAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получает следующий сценарий в последовательности
    /// </summary>
    Task<IScenario?> GetNextScenarioAsync(ScenarioType currentScenario, CancellationToken cancellationToken = default);
}

