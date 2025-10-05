using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Entities.Config;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit;

internal static class _TestConfig
{
    public static IBalanceConfigProvider CreateProvider()
    {
        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        
        var testConfig = new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 10, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection
            {
                MovementCosts = new Dictionary<string, int> { { "Pawn", 1 }, { "King", 1 }, { "Queen", 1 } },
                AttackCost = 1,
                InitialMana = 10,
                MaxMana = 50
            },
                   Pieces = new Dictionary<string, PieceStats>
                   {
                       { "Pawn", new PieceStats { Hp = 10, Atk = 2, Movement = 1, Range = 1, MaxShieldHP = 50, XpToEvolve = 0 } },
                       { "King", new PieceStats { Hp = 50, Atk = 3, Movement = 1, Range = 1, MaxShieldHP = 400, XpToEvolve = 0 } },
                       { "Queen", new PieceStats { Hp = 30, Atk = 7, Movement = 8, Range = 3, MaxShieldHP = 150, XpToEvolve = 0 } },
                       { "Rook", new PieceStats { Hp = 25, Atk = 5, Movement = 8, Range = 8, MaxShieldHP = 100, XpToEvolve = 60 } },
                       { "Bishop", new PieceStats { Hp = 18, Atk = 3, Movement = 8, Range = 4, MaxShieldHP = 80, XpToEvolve = 40 } },
                       { "Knight", new PieceStats { Hp = 20, Atk = 4, Movement = 1, Range = 1, MaxShieldHP = 80, XpToEvolve = 40 } }
                   },
            Abilities = new Dictionary<string, AbilitySpecModel>
            {
                ["Rook.Fortress"] = new AbilitySpecModel 
                { 
                    MpCost = 8, 
                    Cooldown = 5, 
                    Range = 0, 
                    IsAoe = false, 
                    TempHpMultiplier = 2, 
                    DurationTurns = 1 
                },
                ["Queen.Resurrection"] = new AbilitySpecModel 
                { 
                    MpCost = 10, 
                    Cooldown = 10, 
                    Range = 3, 
                    IsAoe = false, 
                    Heal = 20 
                },
                ["King.RoyalCommand"] = new AbilitySpecModel 
                { 
                    MpCost = 10, 
                    Cooldown = 6, 
                    Range = 8, 
                    IsAoe = false 
                },
                ["Bishop.LightArrow"] = new AbilitySpecModel 
                { 
                    MpCost = 5, 
                    Cooldown = 3, 
                    Range = 4, 
                    IsAoe = false, 
                    Damage = 8 
                },
                ["Queen.MagicExplosion"] = new AbilitySpecModel 
                { 
                    MpCost = 8, 
                    Cooldown = 8, 
                    Range = 2, 
                    IsAoe = true, 
                    Damage = 12 
                },
                ["Pawn.ShieldBash"] = new AbilitySpecModel 
                { 
                    MpCost = 2, 
                    Cooldown = 3, 
                    Range = 1, 
                    IsAoe = false, 
                    Damage = 2 
                },
                ["Pawn.Breakthrough"] = new AbilitySpecModel 
                { 
                    MpCost = 2, 
                    Cooldown = 4, 
                    Range = 1, 
                    IsAoe = false, 
                    Damage = 3 
                }
            },
            Evolution = new EvolutionSection
            {
                XpThresholds = new Dictionary<string, int> 
                { 
                    { "Pawn", 0 },
                    { "Knight", 40 },
                    { "Bishop", 40 },
                    { "Rook", 60 }
                },
                Rules = new Dictionary<string, List<string>>
                {
                    { "Pawn", new List<string> { "Knight", "Bishop" } },
                    { "Knight", new List<string> { "Rook" } },
                    { "Bishop", new List<string> { "Rook" } },
                    { "Rook", new List<string> { "Queen" } }
                },
                ImmediateOnLastRank = new Dictionary<string, bool> { { "Pawn", true } }
            },
            Ai = new AiSection(),
            KillRewards = new KillRewardsSection(),
            ShieldSystem = new ShieldSystemConfig
            {
                King = new KingShieldConfig
                {
                    BaseRegen = 5,
                    ProximityBonus1 = new Dictionary<string, int>
                    {
                        { "Pawn", 1 },
                        { "Knight", 2 },
                        { "Bishop", 2 },
                        { "Rook", 3 },
                        { "Queen", 4 },
                        { "King", 0 }
                    },
                    ProximityBonus2 = new Dictionary<string, int>
                    {
                        { "Pawn", 0 },
                        { "Knight", 1 },
                        { "Bishop", 1 },
                        { "Rook", 2 },
                        { "Queen", 3 },
                        { "King", 0 }
                    }
                },
                Ally = new AllyShieldConfig
                {
                    NeighborContribution = new Dictionary<string, int>
                    {
                        { "Pawn", 1 },
                        { "Knight", 2 },
                        { "Bishop", 2 },
                        { "Rook", 3 },
                        { "Queen", 4 },
                        { "King", 5 }
                    }
                }
            }
        };
        
        var testVersion = new BalanceVersion
        {
            Id = Guid.NewGuid(),
            Version = "test",
            Status = "Active",
            CreatedAt = DateTimeOffset.UtcNow,
            Comment = "Test config"
        };
        
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(testVersion);
            
        payloadRepo.Setup(x => x.GetJsonByVersionIdAsync(testVersion.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Json.JsonSerializer.Serialize(testConfig));
            
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        return new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);
    }
}


