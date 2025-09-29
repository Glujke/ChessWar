using ChessWar.Domain.Entities.Config;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для конфигурации маны игрока
/// </summary>
public class PlayerManaConfigTests
{
    [Fact]
    public void PlayerManaSection_ShouldHaveDefaultValues()
    {
        var config = new PlayerManaSection();

        config.InitialMana.Should().Be(10);
        config.MaxMana.Should().Be(50);
        config.ManaRegenPerTurn.Should().Be(10);
        config.MandatoryAction.Should().BeTrue();
        config.MovementCosts.Should().NotBeNull();
        config.MovementCosts.Should().HaveCount(6);
        config.MovementCosts["Pawn"].Should().Be(1);
        config.MovementCosts["Knight"].Should().Be(2);
        config.MovementCosts["Bishop"].Should().Be(3);
        config.MovementCosts["Rook"].Should().Be(3);
        config.MovementCosts["Queen"].Should().Be(4);
        config.MovementCosts["King"].Should().Be(4);
    }

    [Fact]
    public void PlayerManaSection_ShouldAllowCustomValues()
    {
        var config = new PlayerManaSection
        {
            InitialMana = 20,
            MaxMana = 100,
            ManaRegenPerTurn = 15,
            MandatoryAction = false,
            MovementCosts = new Dictionary<string, int>
            {
                ["Pawn"] = 2,
                ["Knight"] = 4,
                ["Bishop"] = 6,
                ["Rook"] = 6,
                ["Queen"] = 8,
                ["King"] = 8
            }
        };

        config.InitialMana.Should().Be(20);
        config.MaxMana.Should().Be(100);
        config.ManaRegenPerTurn.Should().Be(15);
        config.MandatoryAction.Should().BeFalse();
        config.MovementCosts["Pawn"].Should().Be(2);
        config.MovementCosts["King"].Should().Be(8);
    }

    [Fact]
    public void BalanceConfig_ShouldIncludePlayerManaSection()
    {
        var config = new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 5, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection(),
            Pieces = new Dictionary<string, PieceStats>(),
            Abilities = new Dictionary<string, AbilitySpecModel>(),
            Evolution = new EvolutionSection
            {
                XpThresholds = new Dictionary<string, int>(),
                Rules = new Dictionary<string, List<string>>(),
                ImmediateOnLastRank = new Dictionary<string, bool>()
            },
            Ai = new AiSection
            {
                NearEvolutionXp = 19,
                LastRankEdgeY = new Dictionary<string, int>(),
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

        config.PlayerMana.Should().NotBeNull();
        config.PlayerMana.InitialMana.Should().Be(10);
        config.PlayerMana.MaxMana.Should().Be(50);
    }
}
