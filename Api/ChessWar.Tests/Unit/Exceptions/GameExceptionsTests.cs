using FluentAssertions;
using ChessWar.Domain.Exceptions;

namespace ChessWar.Tests.Unit.Exceptions;

public class GameExceptionsTests
{
    [Fact]
    public void InsufficientMpException_Should_Have_Correct_Properties()
    {
        // Arrange
        var requiredMp = 5;
        var availableMp = 3;
        var pieceId = Guid.NewGuid();

        // Act
        var exception = new InsufficientMpException(requiredMp, availableMp, pieceId);

        // Assert
        exception.RequiredMp.Should().Be(requiredMp);
        exception.AvailableMp.Should().Be(availableMp);
        exception.PieceId.Should().Be(pieceId);
        exception.Message.Should().Contain(requiredMp.ToString());
        exception.Message.Should().Contain(availableMp.ToString());
        exception.Message.Should().Contain(pieceId.ToString());
    }

    [Fact]
    public void AbilityOnCooldownException_Should_Have_Correct_Properties()
    {
        // Arrange
        var abilityName = "DoubleStrike";
        var remainingCooldown = 2;
        var pieceId = Guid.NewGuid();

        // Act
        var exception = new AbilityOnCooldownException(abilityName, remainingCooldown, pieceId);

        // Assert
        exception.AbilityName.Should().Be(abilityName);
        exception.RemainingCooldown.Should().Be(remainingCooldown);
        exception.PieceId.Should().Be(pieceId);
        exception.Message.Should().Contain(abilityName);
        exception.Message.Should().Contain(remainingCooldown.ToString());
        exception.Message.Should().Contain(pieceId.ToString());
    }

    [Fact]
    public void OutOfRangeException_Should_Have_Correct_Properties()
    {
        // Arrange
        var maxRange = 3;
        var actualDistance = 5;
        var pieceId = Guid.NewGuid();

        // Act
        var exception = new OutOfRangeException(maxRange, actualDistance, pieceId);

        // Assert
        exception.MaxRange.Should().Be(maxRange);
        exception.ActualDistance.Should().Be(actualDistance);
        exception.PieceId.Should().Be(pieceId);
        exception.Message.Should().Contain(maxRange.ToString());
        exception.Message.Should().Contain(actualDistance.ToString());
        exception.Message.Should().Contain(pieceId.ToString());
    }

    [Fact]
    public void LineOfSightBlockedException_Should_Have_Correct_Properties()
    {
        // Arrange
        var pieceId = Guid.NewGuid();
        var blockingPieceId = Guid.NewGuid();

        // Act
        var exception = new LineOfSightBlockedException(pieceId, blockingPieceId);

        // Assert
        exception.PieceId.Should().Be(pieceId);
        exception.BlockingPieceId.Should().Be(blockingPieceId);
        exception.Message.Should().Contain(pieceId.ToString());
        exception.Message.Should().Contain(blockingPieceId.ToString());
    }

    [Fact]
    public void PieceSwitchForbiddenException_Should_Have_Correct_Properties()
    {
        // Arrange
        var pieceId = Guid.NewGuid();
        var reason = "Turn already started";

        // Act
        var exception = new PieceSwitchForbiddenException(pieceId, reason);

        // Assert
        exception.PieceId.Should().Be(pieceId);
        exception.Reason.Should().Be(reason);
        exception.Message.Should().Contain(pieceId.ToString());
        exception.Message.Should().Contain(reason);
    }

    [Fact]
    public void StageNotCompletedException_Should_Have_Correct_Properties()
    {
        // Arrange
        var stageName = "Battle1";
        var requiredCondition = "Defeat all enemies";

        // Act
        var exception = new StageNotCompletedException(stageName, requiredCondition);

        // Assert
        exception.StageName.Should().Be(stageName);
        exception.RequiredCondition.Should().Be(requiredCondition);
        exception.Message.Should().Contain(stageName);
        exception.Message.Should().Contain(requiredCondition);
    }

    [Fact]
    public void TutorialNotFoundException_Should_Have_Correct_Properties()
    {
        // Arrange
        var tutorialId = Guid.NewGuid();

        // Act
        var exception = new TutorialNotFoundException(tutorialId);

        // Assert
        exception.TutorialId.Should().Be(tutorialId);
        exception.Message.Should().Contain(tutorialId.ToString());
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 5)]
    [InlineData(5, -1)]
    public void InsufficientMpException_Should_Throw_For_Invalid_Values(int requiredMp, int availableMp)
    {
        // Arrange
        var pieceId = Guid.NewGuid();

        // Act & Assert
        var action = () => new InsufficientMpException(requiredMp, availableMp, pieceId);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("", 2)]
    [InlineData(null!, 2)]
    [InlineData("   ", 2)]
    [InlineData("ValidAbility", -1)]
    public void AbilityOnCooldownException_Should_Throw_For_Invalid_Values(string? abilityName, int remainingCooldown)
    {
        // Arrange
        var pieceId = Guid.NewGuid();

        // Act & Assert
        var action = () => new AbilityOnCooldownException(abilityName!, remainingCooldown, pieceId);
        action.Should().Throw<ArgumentException>();
    }
}
