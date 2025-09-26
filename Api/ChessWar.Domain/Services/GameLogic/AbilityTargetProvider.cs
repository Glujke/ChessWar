using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.GameLogic;

/// <summary>
/// Стратегии определения целей для способностей
/// </summary>
public class AbilityTargetProvider : IAbilityTargetProvider
{
    public List<Position> GetTargets(Piece caster, string abilityName, AbilitySpecModel spec, List<Piece> allPieces, IAttackRulesService attackRulesService)
    {
        var targets = new List<Position>();

        switch (abilityName)
        {
            case "ShieldBash":
            {
                foreach (var enemy in allPieces.Where(p => attackRulesService.IsEnemy(caster, p)))
                {
                    var d = attackRulesService.CalculateChebyshevDistance(caster.Position, enemy.Position);
                    if (d <= Math.Max(1, spec.Range))
                        targets.Add(enemy.Position);
                }
                break;
            }
            case "Breakthrough":
            {
                var forwardStep = caster.Team == Team.Elves ? 1 : -1;
                foreach (var enemy in allPieces.Where(p => attackRulesService.IsEnemy(caster, p)))
                {
                    var dx = Math.Abs(enemy.Position.X - caster.Position.X);
                    var dy = enemy.Position.Y - caster.Position.Y;
                    if (dx == 1 && dy == forwardStep)
                        targets.Add(enemy.Position);
                }
                break;
            }
            case "RoyalCommand":
            {
                foreach (var ally in allPieces.Where(p => p.Team == caster.Team && p.Id != caster.Id))
                {
                    var d = attackRulesService.CalculateChebyshevDistance(caster.Position, ally.Position);
                    if (d <= Math.Max(1, spec.Range))
                        targets.Add(ally.Position);
                }
                break;
            }
            default:
                break;
        }

        return targets;
    }
}


