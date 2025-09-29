using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.GameLogic;

/// <summary>
/// Сервис для определения доступных целей для способностей
/// </summary>
public class AbilityTargetService : IAbilityTargetService
{
    private readonly IBalanceConfigProvider _configProvider;
    private readonly IAttackRulesService _attackRulesService;
    private readonly IAbilityTargetProvider _provider;

    public AbilityTargetService(IBalanceConfigProvider configProvider, IAttackRulesService attackRulesService, IAbilityTargetProvider provider)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public List<Position> GetAvailableTargets(Piece piece, string abilityName, IEnumerable<Piece> allPieces)
    {
        var config = _configProvider.GetActive();
        if (!config.Abilities.TryGetValue($"{piece.Type}.{abilityName}", out var spec))
        {
            return new List<Position>();
        }
        return _provider.GetTargets(piece, abilityName, spec, allPieces.ToList(), _attackRulesService);
    }
}


