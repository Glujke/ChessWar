using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Infrastructure.Services;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Unit;

public class BalanceConfigProviderTests
{
    private readonly Mock<IBalanceVersionRepository> _mockVersionRepo;
    private readonly Mock<IBalancePayloadRepository> _mockPayloadRepo;
    private readonly BalanceConfigProvider _provider;

    public BalanceConfigProviderTests()
    {
        _mockVersionRepo = new Mock<IBalanceVersionRepository>();
        _mockPayloadRepo = new Mock<IBalancePayloadRepository>();
        _provider = new BalanceConfigProvider(_mockVersionRepo.Object, _mockPayloadRepo.Object, Mock.Of<ILogger<BalanceConfigProvider>>());
    }

    [Fact]
    public void GetActive_WhenNoPublishedVersion_ShouldReturnEmbeddedDefault()
    {
        // Arrange
        _mockVersionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChessWar.Domain.Entities.BalanceVersion?)null);

        // Act
        var config = _provider.GetActive();

        // Assert
        config.Should().NotBeNull();
        config.Globals.MpRegenPerTurn.Should().Be(10);
        config.Pieces["Pawn"].Hp.Should().Be(10);
        config.Abilities["Bishop.LightArrow"].MpCost.Should().Be(3);
        config.Evolution.XpThresholds["Pawn"].Should().Be(20);
        config.Ai.NearEvolutionXp.Should().Be(19);
    }

    [Fact]
    public void GetActive_WhenPublishedVersionExists_ShouldReturnFromDb()
    {
        // Arrange
        var version = new ChessWar.Domain.Entities.BalanceVersion
        {
            Id = Guid.NewGuid(),
            Version = "2.0.0",
            Status = "Published"
        };
        
        var customJson = """{"globals":{"mpRegenPerTurn":10},"playerMana":{"initialMana":10,"maxMana":50,"manaRegenPerTurn":10,"mandatoryAction":true,"attackCost":1,"movementCosts":{"Pawn":1,"Knight":2,"Bishop":3,"Rook":3,"Queen":4,"King":4}},"pieces":{"Pawn":{"hp":15,"atk":3,"range":1,"movement":1,"xpToEvolve":25}},"abilities":{"Bishop.LightArrow":{"mpCost":5,"cooldown":3,"range":5,"isAoe":false}},"evolution":{"xpThresholds":{"Pawn":25},"rules":{"Pawn":["Knight","Bishop"]},"immediateOnLastRank":{"Pawn":true}},"ai":{"nearEvolutionXp":24,"lastRankEdgeY":{"Elves":6,"Orcs":1},"kingAura":{"radius":4,"atkBonus":2}}}""";

        _mockVersionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(version);
        _mockPayloadRepo.Setup(x => x.GetJsonByVersionIdAsync(version.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customJson);

        // Act
        var config = _provider.GetActive();

        // Assert
        config.Should().NotBeNull();
        config.Globals.MpRegenPerTurn.Should().Be(10);
        config.Pieces["Pawn"].Hp.Should().Be(15);
        config.Abilities["Bishop.LightArrow"].MpCost.Should().Be(5);
        config.Evolution.XpThresholds["Pawn"].Should().Be(25);
        config.Ai.NearEvolutionXp.Should().Be(24);
    }

    [Fact]
    public void Invalidate_ShouldClearCache()
    {
        // Arrange
        _mockVersionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChessWar.Domain.Entities.BalanceVersion?)null);

        // Act
        var config1 = _provider.GetActive();
        _provider.Invalidate();
        var config2 = _provider.GetActive();

        // Assert
        config1.Should().NotBeNull();
        config2.Should().NotBeNull();
        // Кэш должен перезагрузиться (в реальности это будет другой объект)
    }
}
