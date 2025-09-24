using ChessWar.Api.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace ChessWar.Tests.Unit;

public class BalanceConfigValidatorTests
{
    private readonly Mock<ILogger<BalanceConfigValidator>> _loggerMock;
    private readonly BalanceConfigValidator _validator;

    public BalanceConfigValidatorTests()
    {
        _loggerMock = new Mock<ILogger<BalanceConfigValidator>>();
        _validator = new BalanceConfigValidator(_loggerMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidJson_ShouldReturnValid()
    {
        // Arrange
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

        // Act
        var result = await _validator.ValidateAsync(validJson);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidJson_ShouldReturnInvalid()
    {
        // Arrange
        var invalidJson = "invalid json";

        // Act
        var result = await _validator.ValidateAsync(invalidJson);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Invalid JSON format", result.Errors.First());
    }

    [Fact]
    public async Task ValidateAsync_MissingRequiredFields_ShouldReturnInvalid()
    {
        // Arrange
        var invalidJson = """
        {
          "globals": {
            "mpRegenPerTurn": 10
          }
        }
        """;

        // Act
        var result = await _validator.ValidateAsync(invalidJson);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains("Required properties", result.Errors.First());
    }

    [Fact]
    public async Task ValidateAsync_InvalidPieceType_ShouldReturnInvalid()
    {
        // Arrange
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

        // Act
        var result = await _validator.ValidateAsync(invalidJson);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_NegativeValues_ShouldReturnInvalid()
    {
        // Arrange
        var invalidJson = """
        {
          "globals": {
            "mpRegenPerTurn": -1,
            "cooldownTickPhase": "EndTurn"
          },
          "pieces": {
            "Pawn": {
              "hp": 10,
              "atk": 2,
              "range": 1,
              "movement": 1,
              "mp": 5,
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

        // Act
        var result = await _validator.ValidateAsync(invalidJson);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_InvalidCooldownTickPhase_ShouldReturnInvalid()
    {
        // Arrange
        var invalidJson = """
        {
          "globals": {
            "mpRegenPerTurn": 5,
            "cooldownTickPhase": "InvalidPhase"
          },
          "pieces": {
            "Pawn": {
              "hp": 10,
              "atk": 2,
              "range": 1,
              "movement": 1,
              "mp": 5,
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

        // Act
        var result = await _validator.ValidateAsync(invalidJson);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
