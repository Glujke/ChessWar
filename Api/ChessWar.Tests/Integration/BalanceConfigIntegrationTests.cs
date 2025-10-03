using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Entities.Config;
using ChessWar.Infrastructure.Services;
using ChessWar.Persistence.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Text.Json;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class BalanceConfigIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ChessWarDbContext _context;
    private readonly IBalanceConfigProvider _configProvider;

    public BalanceConfigIntegrationTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ChessWarDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

        services.AddScoped<IBalanceVersionRepository, ChessWar.Infrastructure.Repositories.BalanceVersionRepository>();
        services.AddScoped<IBalancePayloadRepository, ChessWar.Infrastructure.Repositories.BalancePayloadRepository>();
        services.AddSingleton<IBalanceConfigProvider, BalanceConfigProvider>();
        services.AddScoped<IBalanceConfigValidator, ChessWar.Api.Services.BalanceConfigValidator>();
        services.AddLogging();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ChessWarDbContext>();
        _configProvider = _serviceProvider.GetRequiredService<IBalanceConfigProvider>();

        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task CreateAndPublishConfig_ShouldBeAvailableThroughProvider()
    {
        var configService = new ChessWar.Application.Services.Configuration.ConfigService(
            _serviceProvider.GetRequiredService<IBalanceVersionRepository>(),
            Mock.Of<IPieceConfigService>(),
            _serviceProvider.GetRequiredService<IBalancePayloadRepository>(),
            _serviceProvider.GetRequiredService<IBalanceConfigValidator>());

        var customConfig = new BalanceConfig
        {
            Globals = new GlobalsSection { MpRegenPerTurn = 8, CooldownTickPhase = "EndTurn" },
            PlayerMana = new PlayerManaSection
            {
                InitialMana = 10,
                MaxMana = 50,
                ManaRegenPerTurn = 10,
                MandatoryAction = true,
                MovementCosts = new Dictionary<string, int> { ["Pawn"] = 1 }
            },
            Pieces = new Dictionary<string, PieceStats>
            {
                ["Pawn"] = new PieceStats { Hp = 12, Atk = 3, Range = 1, Movement = 1, XpToEvolve = 25 }
            },
            Abilities = new Dictionary<string, AbilitySpecModel>
            {
                ["Bishop.LightArrow"] = new AbilitySpecModel { MpCost = 4, Cooldown = 2, Range = 5, IsAoe = false }
            },
            Evolution = new EvolutionSection
            {
                XpThresholds = new Dictionary<string, int> { ["Pawn"] = 25 },
                Rules = new Dictionary<string, List<string>> { ["Pawn"] = new() { "Knight", "Bishop" } },
                ImmediateOnLastRank = new Dictionary<string, bool> { ["Pawn"] = true }
            },
            Ai = new AiSection
            {
                NearEvolutionXp = 24,
                LastRankEdgeY = new Dictionary<string, int> { ["Elves"] = 6, ["Orcs"] = 1 },
                KingAura = new KingAuraConfig { Radius = 4, AtkBonus = 2 }
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

        var version = await configService.CreateConfigVersionAsync("2.0.0", "Test config", CancellationToken.None);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        await configService.SavePayloadAsync(version.Id, JsonSerializer.Serialize(customConfig, jsonOptions), CancellationToken.None);
        await configService.PublishConfigVersionAsync(version.Id, CancellationToken.None);

        var activeConfig = _configProvider.GetActive();
        activeConfig.Should().NotBeNull();
        activeConfig.Globals.MpRegenPerTurn.Should().Be(8);
        activeConfig.Pieces["Pawn"].Hp.Should().Be(12);
        activeConfig.Abilities["Bishop.LightArrow"].MpCost.Should().Be(4);
        activeConfig.Evolution.XpThresholds["Pawn"].Should().Be(25);
        activeConfig.Ai.NearEvolutionXp.Should().Be(24);
    }

    [Fact]
    public void GetActive_WhenNoPublishedVersion_ShouldReturnEmbeddedDefault()
    {
        var config = _configProvider.GetActive();

        config.Should().NotBeNull();
        config.Globals.MpRegenPerTurn.Should().Be(10);
        config.PlayerMana.MaxMana.Should().Be(50);
        config.Pieces["Pawn"].Hp.Should().Be(10);
        config.Abilities["Bishop.LightArrow"].MpCost.Should().Be(3);
        config.Evolution.XpThresholds["Pawn"].Should().Be(20);
        config.Ai.NearEvolutionXp.Should().Be(19);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
