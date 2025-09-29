using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Interfaces.GameLogic;

/// <summary>
/// Провайдер стратегий вычисления целей для способностей
/// </summary>
public interface IAbilityTargetProvider
{
    /// <summary>
    /// Возвращает доступные цели для указанной способности
    /// </summary>
    List<Position> GetTargets(Piece caster, string abilityName, AbilitySpecModel spec, List<Piece> allPieces, IAttackRulesService attackRulesService);
}


