using ChessWar.Domain.Entities;
using ChessWar.Domain.Events;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Domain.Services.GameLogic;

public class AbilityService : IAbilityService
{
    private readonly IBalanceConfigProvider _configProvider;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IPieceDomainService _pieceDomainService;

    public AbilityService(IBalanceConfigProvider configProvider, IDomainEventDispatcher eventDispatcher, IPieceDomainService pieceDomainService)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _pieceDomainService = pieceDomainService ?? throw new ArgumentNullException(nameof(pieceDomainService));
    }

    public bool CanUseAbility(Piece piece, string abilityName, Position target, List<Piece> allPieces)
    {
        var config = _configProvider.GetActive();
        var key = $"{piece.Type}.{abilityName}";
        
        if (!config.Abilities.TryGetValue(key, out var spec)) return false;
        if (piece.AbilityCooldowns.GetValueOrDefault(abilityName, 0) > 0) return false;

        var distance = CalculateChebyshevDistance(piece.Position, target);
        if (distance > spec.Range) return false;

        if (!spec.IsAoe)
        {
            if (IsStraightOrDiagonal(piece.Position, target))
            {
                if (IsPathBlocked(piece.Position, target, allPieces)) return false;
            }
        }
        
        var owner = piece.Owner;
        if (owner == null) return false;
        if (!owner.CanSpend(spec.MpCost)) return false;
        return true;
    }

    public bool UseAbility(Piece piece, string abilityName, Position target, List<Piece> allPieces)
    {
        if (!CanUseAbility(piece, abilityName, target, allPieces)) return false;
        
        var config = _configProvider.GetActive();
        var key = $"{piece.Type}.{abilityName}";
        var spec = config.Abilities[key];

        piece.Owner!.Spend(spec.MpCost);
        if (spec.Cooldown > 0)
        {
            _pieceDomainService.SetAbilityCooldown(piece, abilityName, spec.Cooldown);
        }

        if (abilityName == "LightArrow")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team != piece.Team)
            {
                _pieceDomainService.TakeDamage(targetPiece, Math.Max(0, spec.Damage));
                
                if (_pieceDomainService.IsDead(targetPiece))
                {
                    _eventDispatcher.Publish(new PieceKilledEvent(piece, targetPiece));
                    _eventDispatcher.PublishAll();
                }
            }
        }
        else if (abilityName == "Heal")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team == piece.Team)
            {
                _pieceDomainService.Heal(targetPiece, Math.Max(0, spec.Heal));
            }
        }
        else if (abilityName == "MagicExplosion")
        {
            foreach (var tp in allPieces)
            {
                var d = CalculateChebyshevDistance(piece.Position, tp.Position);
                if (d <= spec.Range && tp.Team != piece.Team)
                {
                    _pieceDomainService.TakeDamage(tp, Math.Max(0, spec.Damage));
                }
            }
        }
        else if (abilityName == "Resurrection")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team == piece.Team)
            {
                var half = Math.Max(1, _pieceDomainService.GetMaxHP(targetPiece.Type) / 2);
                if (!targetPiece.IsAlive || targetPiece.HP < half)
                {
                    _pieceDomainService.Heal(targetPiece, half - Math.Max(0, targetPiece.HP));
                }
            }
        }
        else if (abilityName == "DoubleStrike")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team != piece.Team)
            {
                var hits = Math.Max(1, spec.Hits);
                var dmg = Math.Max(0, spec.DamagePerHit);
                for (var i = 0; i < hits; i++)
                {
                    _pieceDomainService.TakeDamage(targetPiece, dmg);
                    if (_pieceDomainService.IsDead(targetPiece)) break;
                }
                
                if (_pieceDomainService.IsDead(targetPiece))
                {
                    _eventDispatcher.Publish(new PieceKilledEvent(piece, targetPiece));
                    _eventDispatcher.PublishAll();
                }
            }
        }
        else if (abilityName == "Fortress")
        {
            var mult = Math.Max(1, spec.TempHpMultiplier);
            var newHp = Math.Min(_pieceDomainService.GetMaxHP(piece.Type) * mult, piece.HP * mult);
            piece.HP = newHp;
            _pieceDomainService.SetAbilityCooldown(piece, "__FortressBuff", Math.Max(1, spec.DurationTurns));
        }
        else if (abilityName == "RoyalCommand")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team == piece.Team)
            {
                _pieceDomainService.SetAbilityCooldown(targetPiece, "__RoyalCommandGranted", 2); 
                _pieceDomainService.SetAbilityCooldown(targetPiece, "__RoyalCommandFreeAction", 2); 
            }
        }
        else if (abilityName == "ShieldBash")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team != piece.Team)
            {
                _pieceDomainService.TakeDamage(targetPiece, Math.Max(0, spec.Damage));
                
                if (_pieceDomainService.IsDead(targetPiece))
                {
                    _eventDispatcher.Publish(new PieceKilledEvent(piece, targetPiece));
                    _eventDispatcher.PublishAll();
                }
            }
        }
        else if (abilityName == "Breakthrough")
        {
            var targetPiece = allPieces.FirstOrDefault(p => p.Position.X == target.X && p.Position.Y == target.Y);
            if (targetPiece != null && targetPiece.Team != piece.Team)
            {
                _pieceDomainService.TakeDamage(targetPiece, Math.Max(0, spec.Damage));
                
                if (_pieceDomainService.IsDead(targetPiece))
                {
                    _eventDispatcher.Publish(new PieceKilledEvent(piece, targetPiece));
                    _eventDispatcher.PublishAll(); 
                }
            }
        }
        else if (abilityName == "KingAura")
        {
            // Аура короля - пассивная способность, которая увеличивает атаку союзников в радиусе
            var balanceConfig = _configProvider.GetActive();
            var kingAuraConfig = balanceConfig.Ai.KingAura;
            if (kingAuraConfig != null)
            {
                var radius = kingAuraConfig.Radius;
                var atkBonus = kingAuraConfig.AtkBonus;
                
                // Находим всех союзников в радиусе
                var alliesInRange = allPieces.Where(p => 
                    p.Owner == piece.Owner && 
                    p.Id != piece.Id && 
                    CalculateChebyshevDistance(piece.Position, p.Position) <= radius)
                    .ToList();
                
                // Увеличиваем атаку союзников
                foreach (var ally in alliesInRange)
                {
                    ally.ATK += atkBonus;
                }
            }
        }
        return true;
    }

    private static int CalculateChebyshevDistance(Position a, Position b)
        => Math.Max(Math.Abs(a.X - b.X), Math.Abs(a.Y - b.Y));

    private static bool IsPathBlocked(Position from, Position to, List<Piece> all)
    {
        // Создаем словарь позиций для быстрого поиска
        var occupiedPositions = new HashSet<(int, int)>();
        foreach (var piece in all)
        {
            occupiedPositions.Add((piece.Position.X, piece.Position.Y));
        }
        
        var dx = Math.Sign(to.X - from.X);
        var dy = Math.Sign(to.Y - from.Y);
        var steps = Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
        var x = from.X; var y = from.Y;
        for (int i = 1; i < steps; i++)
        {
            x += dx; y += dy;
            if (occupiedPositions.Contains((x, y))) return true;
        }
        return false;
    }

    private static bool IsStraightOrDiagonal(Position a, Position b)
    {
        var dx = Math.Abs(a.X - b.X);
        var dy = Math.Abs(a.Y - b.Y);
        return dx == 0 || dy == 0 || dx == dy;
    }
}


