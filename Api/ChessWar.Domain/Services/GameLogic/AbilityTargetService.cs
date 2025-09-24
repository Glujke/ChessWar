using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.GameLogic
{
    /// <summary>
    /// Реализация вычисления целей для способностей (ShieldBash, Breakthrough и др.).
    /// </summary>
    public sealed class AbilityTargetService : IAbilityTargetService
    {
        private readonly IBalanceConfigProvider _configProvider;
        private readonly IAttackRulesService _attackRulesService;

        public AbilityTargetService(IBalanceConfigProvider configProvider, IAttackRulesService attackRulesService)
        {
            _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
        }

        public List<Position> GetAvailableTargets(Piece piece, string abilityName, IEnumerable<Piece> allPieces)
        {
            var pieces = allPieces.Where(p => p.IsAlive).ToList();
            var key = $"{piece.Type}.{abilityName}";
            var config = _configProvider.GetActive();
            if (!config.Abilities.TryGetValue(key, out var spec))
            {
                return new List<Position>();
            }

            if (abilityName == "ShieldBash")
            {
                var radius = 1;
                var result = new List<Position>();
                for (var dx = -radius; dx <= radius; dx++)
                {
                    for (var dy = -radius; dy <= radius; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        var pos = new Position(piece.Position.X + dx, piece.Position.Y + dy);
                        if (_attackRulesService.CanAttack(piece, pos, pieces))
                        {
                            result.Add(pos);
                        }
                    }
                }
                return result;
            }

            if (abilityName == "Breakthrough")
            {
                var forward = piece.Team == Enums.Team.Elves ? 1 : -1;
                var candidates = new[]
                {
                    new Position(piece.Position.X, piece.Position.Y + forward),
                    new Position(piece.Position.X - 1, piece.Position.Y + forward),
                    new Position(piece.Position.X + 1, piece.Position.Y + forward)
                };
                return candidates.Where(p => _attackRulesService.CanAttack(piece, p, pieces)).ToList();
            }

            return new List<Position>();
        }
    }
}


