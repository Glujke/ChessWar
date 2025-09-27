using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Entities.Scenarios;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;

namespace ChessWar.Application.Services.Tutorial;

/// <summary>
/// Сервис для управления сценариями обучения
/// </summary>
public class ScenarioTutorialService : IScenarioService
{
    public async Task<IScenario> CreateBattleScenarioAsync(AiDifficulty difficulty, CancellationToken cancellationToken = default)
    {
        var scenario = new BattleScenario(difficulty, showHints: true);
        
        await Task.Delay(1, cancellationToken);
        
        return scenario;
    }

    public async Task<IScenario> CreateBossScenarioAsync(CancellationToken cancellationToken = default)
    {
        var scenario = new BossScenario(BossType.KingAndQueen, showHints: true);
        
        await Task.Delay(1, cancellationToken);
        
        return scenario;
    }

    public async Task<IScenario?> GetNextScenarioAsync(ScenarioType currentScenario, CancellationToken cancellationToken = default)
    {
        await Task.Delay(1, cancellationToken);
        
        return currentScenario switch
        {
            ScenarioType.Battle => new BossScenario(BossType.KingAndQueen, showHints: true),
            ScenarioType.Boss => null, 
            _ => null
        };
    }
}
