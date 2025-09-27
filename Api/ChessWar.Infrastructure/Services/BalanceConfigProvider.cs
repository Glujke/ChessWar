using System.Text.Json;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using Microsoft.Extensions.Logging;

namespace ChessWar.Infrastructure.Services;

public sealed class BalanceConfigProvider : IBalanceConfigProvider
{
    private readonly IBalanceVersionRepository _repo;
    private readonly IBalancePayloadRepository _payloadRepo;
    private readonly ILogger<BalanceConfigProvider> _logger;
    private BalanceConfig? _cached;
    private DateTimeOffset _loadedAt;

    public BalanceConfigProvider(IBalanceVersionRepository repo, IBalancePayloadRepository payloadRepo, ILogger<BalanceConfigProvider> logger)
    {
        _repo = repo;
        _payloadRepo = payloadRepo;
        _logger = logger;
    }

    public BalanceConfig GetActive()
    {
        if (_cached != null) return _cached;

        try
        {
            var active = _repo.GetActiveAsync().GetAwaiter().GetResult();
            if (active != null)
            {
                var json = _payloadRepo.GetJsonByVersionIdAsync(active.Id).GetAwaiter().GetResult();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var cfg = JsonSerializer.Deserialize<BalanceConfig>(json!, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (cfg != null)
                    {
                        _cached = cfg;
                        _loadedAt = DateTimeOffset.UtcNow;
                        return _cached;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load active balance config from DB, fallback to embedded");
        }

        _cached = GetEmbeddedDefault();
        _loadedAt = DateTimeOffset.UtcNow;
        return _cached;
    }

    public void Invalidate()
    {
        _cached = null;
    }

    private static BalanceConfig GetEmbeddedDefault()
    {
        return new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 10, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection
            {
                InitialMana = 10,
                MaxMana = 50,
                ManaRegenPerTurn = 10,
                MandatoryAction = true,
                AttackCost = 1,
                MovementCosts = new Dictionary<string, int>
                {
                    ["Pawn"] = 1,
                    ["Knight"] = 2,
                    ["Bishop"] = 3,
                    ["Rook"] = 3,
                    ["Queen"] = 4,
                    ["King"] = 4
                }
            },
            Pieces = new Dictionary<string, PieceStats>
            {
                ["Pawn"]   = new PieceStats{ Hp=10, Atk=2, Range=1, Movement=1, XpToEvolve=20 },
                ["Knight"] = new PieceStats{ Hp=20, Atk=4, Range=1, Movement=1, XpToEvolve=40 },
                ["Bishop"] = new PieceStats{ Hp=18, Atk=3, Range=4, Movement=8, XpToEvolve=40 },
                ["Rook"]   = new PieceStats{ Hp=25, Atk=5, Range=8, Movement=8, XpToEvolve=60 },
                ["Queen"]  = new PieceStats{ Hp=30, Atk=7, Range=3, Movement=8, XpToEvolve=0 },
                ["King"]   = new PieceStats{ Hp=50, Atk=3, Range=1, Movement=1, XpToEvolve=0 },
            },
            Abilities = new Dictionary<string, AbilitySpecModel>
            {
                ["Bishop.LightArrow"]    = new AbilitySpecModel{ MpCost=3, Cooldown=2, Range=4, IsAoe=false, Damage=4 },
                ["Bishop.Heal"]          = new AbilitySpecModel{ MpCost=6, Cooldown=4, Range=2, IsAoe=false, Heal=5 },
                ["Knight.DoubleStrike"]  = new AbilitySpecModel{ MpCost=5, Cooldown=3, Range=1, IsAoe=false, Hits=2, DamagePerHit=3 },
                ["Rook.Fortress"]        = new AbilitySpecModel{ MpCost=8, Cooldown=5, Range=0, IsAoe=false, TempHpMultiplier=2, DurationTurns=1 },
                ["Queen.MagicExplosion"] = new AbilitySpecModel{ MpCost=10, Cooldown=3, Range=3, IsAoe=true, Damage=7 },
                ["Queen.Resurrection"]   = new AbilitySpecModel{ MpCost=10, Cooldown=10, Range=3, IsAoe=false, Heal=20 },
                ["King.RoyalCommand"]    = new AbilitySpecModel{ MpCost=10, Cooldown=6, Range=8, IsAoe=false },
                ["Pawn.ShieldBash"]      = new AbilitySpecModel{ MpCost=2, Cooldown=0, Range=1, IsAoe=false, Damage=2 },
                ["Pawn.Breakthrough"]    = new AbilitySpecModel{ MpCost=2, Cooldown=0, Range=1, IsAoe=false, Damage=3 },
            },
            Evolution = new EvolutionSection
            {
                XpThresholds = new Dictionary<string, int>
                {
                    ["Pawn"] = 20, ["Knight"] = 40, ["Bishop"] = 40, ["Rook"] = 60, ["Queen"] = 0, ["King"] = 0
                },
                Rules = new Dictionary<string, List<string>>
                {
                    ["Pawn"] = new() { "Knight", "Bishop" },
                    ["Knight"] = new() { "Rook" },
                    ["Bishop"] = new() { "Rook" },
                    ["Rook"] = new() { "Queen" },
                    ["Queen"] = new(),
                    ["King"] = new()
                },
                ImmediateOnLastRank = new Dictionary<string, bool> { ["Pawn"] = true }
            },
            Ai = new AiSection
            {
                NearEvolutionXp = 19,
                LastRankEdgeY = new Dictionary<string, int> { ["Elves"] = 6, ["Orcs"] = 1 },
                KingAura = new KingAuraConfig { Radius = 3, AtkBonus = 1 }
            },
            KillRewards = new KillRewardsSection
            {
                Pawn = 10,
                Knight = 20,
                Bishop = 20,
                Rook = 30,
                Queen = 50,
                King = 100
            }
        };
    }
}


