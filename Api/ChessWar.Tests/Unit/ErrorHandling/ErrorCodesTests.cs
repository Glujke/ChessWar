using FluentAssertions;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit.ErrorHandling;

public class ErrorCodesTests
{
    [Fact]
    public void Should_Have_All_Required_Error_Codes()
    {
        var errorCodes = ErrorCodes.GetAll();

        errorCodes.Should().Contain(ErrorCodes.InsufficientMp);
        errorCodes.Should().Contain(ErrorCodes.AbilityOnCooldown);
        errorCodes.Should().Contain(ErrorCodes.OutOfRange);
        errorCodes.Should().Contain(ErrorCodes.LineOfSightBlocked);
        errorCodes.Should().Contain(ErrorCodes.PieceSwitchForbidden);
        errorCodes.Should().Contain(ErrorCodes.StageNotCompleted);
        errorCodes.Should().Contain(ErrorCodes.TutorialNotFound);
    }

    [Fact]
    public void Should_Have_Unique_Error_Codes()
    {
        var errorCodes = ErrorCodes.GetAll();

        errorCodes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Should_Have_Non_Empty_Error_Codes()
    {
        var errorCodes = ErrorCodes.GetAll();

        errorCodes.Should().NotBeEmpty();
        errorCodes.Should().AllSatisfy(code => code.ToString().Should().NotBeNullOrWhiteSpace());
    }

    [Theory]
    [InlineData("InsufficientMp", "Insufficient MP to perform this action")]
    [InlineData("AbilityOnCooldown", "Ability is currently on cooldown")]
    [InlineData("OutOfRange", "Target is out of range")]
    [InlineData("LineOfSightBlocked", "Line of sight is blocked")]
    [InlineData("PieceSwitchForbidden", "Cannot switch pieces during this turn")]
    [InlineData("StageNotCompleted", "Current stage is not completed")]
    [InlineData("TutorialNotFound", "Tutorial session not found")]
    public void Should_Have_Correct_Error_Code_Values(string expectedCode, string expectedDescription)
    {
        var errorCode = ErrorCodes.GetByCode(expectedCode);

        errorCode.Code.Should().Be(expectedCode);
        errorCode.Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void Should_Throw_When_Error_Code_Not_Found()
    {
        var nonExistentCode = "NonExistentCode";

        var action = () => ErrorCodes.GetByCode(nonExistentCode);
        action.Should().Throw<ArgumentException>()
            .WithMessage($"Error code '{nonExistentCode}' not found*");
    }

    [Theory]
    [InlineData("InsufficientMp", true)]
    [InlineData("AbilityOnCooldown", true)]
    [InlineData("OutOfRange", true)]
    [InlineData("LineOfSightBlocked", true)]
    [InlineData("PieceSwitchForbidden", true)]
    [InlineData("StageNotCompleted", true)]
    [InlineData("TutorialNotFound", true)]
    [InlineData("NonExistentCode", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("   ", false)]
    public void Should_Check_If_Error_Code_Exists(string? code, bool expectedExists)
    {
        var exists = ErrorCodes.Exists(code);

        exists.Should().Be(expectedExists);
    }

    [Fact]
    public void Should_Throw_When_Code_Is_Null_Or_WhiteSpace()
    {
        var nullCode = (string)null!;
        var emptyCode = "";
        var whitespaceCode = "   ";

        var nullAction = () => ErrorCodes.GetByCode(nullCode);
        nullAction.Should().Throw<ArgumentException>()
            .WithParameterName("code");

        var emptyAction = () => ErrorCodes.GetByCode(emptyCode);
        emptyAction.Should().Throw<ArgumentException>()
            .WithParameterName("code");

        var whitespaceAction = () => ErrorCodes.GetByCode(whitespaceCode);
        whitespaceAction.Should().Throw<ArgumentException>()
            .WithParameterName("code");
    }
}
