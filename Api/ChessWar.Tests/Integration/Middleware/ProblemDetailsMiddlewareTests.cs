using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Api.Middleware;
using ChessWar.Domain.Exceptions;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Integration.Middleware;

public class ProblemDetailsMiddlewareTests
{
    private readonly Mock<ILogger<ProblemDetailsMiddleware>> _loggerMock;
    private readonly ProblemDetailsMiddleware _middleware;

    public ProblemDetailsMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ProblemDetailsMiddleware>>();
        _middleware = new ProblemDetailsMiddleware(_ => Task.CompletedTask, _loggerMock.Object);
    }

    [Fact]
    public async Task Should_Handle_InsufficientMpException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var requiredMp = 5;
        var availableMp = 3;
        var pieceId = Guid.NewGuid();
        var exception = new InsufficientMpException(requiredMp, availableMp, pieceId);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
        problemDetails.Title.Should().Be("Bad Request");
        problemDetails.Detail.Should().Contain(requiredMp.ToString());
        problemDetails.Detail.Should().Contain(availableMp.ToString());
        problemDetails.Detail.Should().Contain(pieceId.ToString());
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.InsufficientMp.Code);
    }

    [Fact]
    public async Task Should_Handle_AbilityOnCooldownException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var abilityName = "DoubleStrike";
        var remainingCooldown = 2;
        var pieceId = Guid.NewGuid();
        var exception = new AbilityOnCooldownException(abilityName, remainingCooldown, pieceId);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Contain(abilityName);
        problemDetails.Detail.Should().Contain(remainingCooldown.ToString());
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.AbilityOnCooldown.Code);
    }

    [Fact]
    public async Task Should_Handle_OutOfRangeException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var maxRange = 3;
        var actualDistance = 5;
        var pieceId = Guid.NewGuid();
        var exception = new OutOfRangeException(maxRange, actualDistance, pieceId);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Contain(maxRange.ToString());
        problemDetails.Detail.Should().Contain(actualDistance.ToString());
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.OutOfRange.Code);
    }

    [Fact]
    public async Task Should_Handle_LineOfSightBlockedException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var pieceId = Guid.NewGuid();
        var blockingPieceId = Guid.NewGuid();
        var exception = new LineOfSightBlockedException(pieceId, blockingPieceId);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Contain(pieceId.ToString());
        problemDetails.Detail.Should().Contain(blockingPieceId.ToString());
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.LineOfSightBlocked.Code);
    }

    [Fact]
    public async Task Should_Handle_PieceSwitchForbiddenException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var pieceId = Guid.NewGuid();
        var reason = "Turn already started";
        var exception = new PieceSwitchForbiddenException(pieceId, reason);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.BadRequest);
        problemDetails.Detail.Should().Contain(pieceId.ToString());
        problemDetails.Detail.Should().Contain(reason);
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.PieceSwitchForbidden.Code);
    }

    [Fact]
    public async Task Should_Handle_StageNotCompletedException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var stageName = "Battle1";
        var requiredCondition = "Defeat all enemies";
        var exception = new StageNotCompletedException(stageName, requiredCondition);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.Conflict);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.Conflict);
        problemDetails.Detail.Should().Contain(stageName);
        problemDetails.Detail.Should().Contain(requiredCondition);
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.StageNotCompleted.Code);
    }

    [Fact]
    public async Task Should_Handle_TutorialNotFoundException_With_Correct_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var tutorialId = Guid.NewGuid();
        var exception = new TutorialNotFoundException(tutorialId);

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.NotFound);
        problemDetails.Detail.Should().Contain(tutorialId.ToString());
        problemDetails.Extensions.Should().ContainKey("errorCode");
        problemDetails.Extensions["errorCode"]!.ToString().Should().Be(ErrorCodes.TutorialNotFound.Code);
    }

    [Fact]
    public async Task Should_Handle_Generic_Exception_With_Default_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Status.Should().Be((int)HttpStatusCode.InternalServerError);
        problemDetails.Title.Should().Be("An error occurred while processing your request.");
        problemDetails.Detail.Should().Contain("Something went wrong");
        problemDetails.Extensions.Should().NotContainKey("errorCode");
    }

    [Fact]
    public async Task Should_Include_TraceId_In_ProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext();
        var traceId = "test-trace-id";
        context.TraceIdentifier = traceId;
        var exception = new InsufficientMpException(5, 3, Guid.NewGuid());

        // Act
        await _middleware.HandleExceptionAsync(context, exception);

        // Assert
        var problemDetails = await GetProblemDetailsFromResponse(context);
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions["traceId"]!.ToString().Should().Be(traceId);
    }

    private static HttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ProblemDetails> GetProblemDetailsFromResponse(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<ProblemDetails>(responseBody)!;
    }
}
