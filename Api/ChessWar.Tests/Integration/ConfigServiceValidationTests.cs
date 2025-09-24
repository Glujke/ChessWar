using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Application.Services.Configuration;
using ChessWar.Infrastructure.Repositories;
using ChessWar.Persistence.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChessWar.Tests.Integration;

public class ConfigServiceValidationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ChessWarDbContext _context;
    private readonly IConfigService _configService;

    public ConfigServiceValidationTests()
    {
        var services = new ServiceCollection();
        
        // Настройка БД в памяти
        services.AddDbContext<ChessWarDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Регистрация сервисов
        services.AddScoped<IBalanceVersionRepository, BalanceVersionRepository>();
        services.AddScoped<IBalancePayloadRepository, BalancePayloadRepository>();
        services.AddMemoryCache();
        services.AddScoped<IPieceConfigService, ChessWar.Infrastructure.Services.PieceConfigService>();
        services.AddScoped<IBalanceConfigValidator, ChessWar.Api.Services.BalanceConfigValidator>();
        services.AddScoped<IConfigService, ConfigService>();
        services.AddLogging();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ChessWarDbContext>();
        _configService = _serviceProvider.GetRequiredService<IConfigService>();
        
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task SavePayloadAsync_ValidJson_ShouldSucceed()
    {
        // Arrange
        var version = await _configService.CreateConfigVersionAsync("1.0.0", "Test version");
        
        var validJson = """
        {
          "globals": {
            "mpRegenPerTurn": 10,
            "cooldownTickPhase": "EndTurn"
          },
          "playerMana": {
            "initialMana": 10,
            "maxMana": 50,
            "manaRegenPerTurn": 10,
            "mandatoryAction": true,
            "attackCost": 1,
            "movementCosts": {
              "Pawn": 1,
              "Knight": 2,
              "Bishop": 3,
              "Rook": 3,
              "Queen": 4,
              "King": 4
            }
          },
          "pieces": {
            "Pawn": {
              "hp": 10,
              "atk": 2,
              "range": 1,
              "movement": 1,
              "xpToEvolve": 20
            }
          },
          "abilities": {
            "Pawn.ShieldBash": {
              "mpCost": 2,
              "cooldown": 0,
              "range": 1,
              "isAoe": false,
              "damage": 2
            }
          },
          "evolution": {
            "xpThresholds": {
              "Pawn": 20
            },
            "rules": {
              "Pawn": ["Knight", "Bishop"]
            }
          },
          "ai": {
            "nearEvolutionXp": 19
          }
        }
        """;

        // Act & Assert
        await _configService.SavePayloadAsync(version.Id, validJson);
        
        // Verify payload was saved
        var savedPayload = await _configService.GetPayloadAsync(version.Id);
        Assert.NotNull(savedPayload);
        Assert.Contains("Pawn", savedPayload);
    }

    [Fact]
    public async Task SavePayloadAsync_InvalidJson_ShouldThrowException()
    {
        // Arrange
        var version = await _configService.CreateConfigVersionAsync("1.0.0", "Test version");
        var invalidJson = "invalid json";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _configService.SavePayloadAsync(version.Id, invalidJson));
    }

    [Fact]
    public async Task SavePayloadAsync_MissingRequiredFields_ShouldThrowException()
    {
        // Arrange
        var version = await _configService.CreateConfigVersionAsync("1.0.0", "Test version");
        var invalidJson = """
        {
          "globals": {
            "mpRegenPerTurn": 5
          }
        }
        """;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _configService.SavePayloadAsync(version.Id, invalidJson));
    }

    [Fact]
    public async Task SavePayloadAsync_InvalidPieceType_ShouldThrowException()
    {
        // Arrange
        var version = await _configService.CreateConfigVersionAsync("1.0.0", "Test version");
        var invalidJson = """
        {
          "globals": {
            "mpRegenPerTurn": 10,
            "cooldownTickPhase": "EndTurn"
          },
          "playerMana": {
            "initialMana": 10,
            "maxMana": 50,
            "manaRegenPerTurn": 10,
            "mandatoryAction": true,
            "attackCost": 1,
            "movementCosts": {
              "Pawn": 1
            }
          },
          "pieces": {
            "InvalidPiece": {
              "hp": 10,
              "atk": 2,
              "range": 1,
              "movement": 1,
              "xpToEvolve": 20
            }
          },
          "abilities": {},
          "evolution": {
            "xpThresholds": {},
            "rules": {}
          },
          "ai": {
            "nearEvolutionXp": 19
          }
        }
        """;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _configService.SavePayloadAsync(version.Id, invalidJson));
    }

    public void Dispose()
    {
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
