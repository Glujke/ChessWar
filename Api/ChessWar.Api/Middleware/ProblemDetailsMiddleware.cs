using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ChessWar.Domain.Exceptions;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Api.Middleware;

/// <summary>
/// Middleware for handling exceptions and converting them to RFC 7807 ProblemDetails
/// </summary>
public class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions and converts them to ProblemDetails
    /// </summary>
    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An exception occurred: {ExceptionType}", exception.GetType().Name);

        var problemDetails = CreateProblemDetails(context, exception);
        
        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = context.TraceIdentifier
            }
        };

        switch (exception)
        {
            case InsufficientMpException insufficientMpEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.InsufficientMp.Code;
                problemDetails.Extensions["requiredMp"] = insufficientMpEx.RequiredMp;
                problemDetails.Extensions["availableMp"] = insufficientMpEx.AvailableMp;
                problemDetails.Extensions["pieceId"] = insufficientMpEx.PieceId;
                break;

            case AbilityOnCooldownException cooldownEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.AbilityOnCooldown.Code;
                problemDetails.Extensions["abilityName"] = cooldownEx.AbilityName;
                problemDetails.Extensions["remainingCooldown"] = cooldownEx.RemainingCooldown;
                problemDetails.Extensions["pieceId"] = cooldownEx.PieceId;
                break;

            case OutOfRangeException outOfRangeEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.OutOfRange.Code;
                problemDetails.Extensions["maxRange"] = outOfRangeEx.MaxRange;
                problemDetails.Extensions["actualDistance"] = outOfRangeEx.ActualDistance;
                problemDetails.Extensions["pieceId"] = outOfRangeEx.PieceId;
                break;

            case LineOfSightBlockedException losEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.LineOfSightBlocked.Code;
                problemDetails.Extensions["pieceId"] = losEx.PieceId;
                problemDetails.Extensions["blockingPieceId"] = losEx.BlockingPieceId;
                break;

            case PieceSwitchForbiddenException switchEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                problemDetails.Title = "Bad Request";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.PieceSwitchForbidden.Code;
                problemDetails.Extensions["pieceId"] = switchEx.PieceId;
                problemDetails.Extensions["reason"] = switchEx.Reason;
                break;

            case StageNotCompletedException stageEx:
                problemDetails.Status = (int)HttpStatusCode.Conflict;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.9";
                problemDetails.Title = "Conflict";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.StageNotCompleted.Code;
                problemDetails.Extensions["stageName"] = stageEx.StageName;
                problemDetails.Extensions["requiredCondition"] = stageEx.RequiredCondition;
                break;

            case TutorialNotFoundException tutorialEx:
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                problemDetails.Title = "Not Found";
                problemDetails.Detail = exception.Message;
                problemDetails.Extensions["errorCode"] = ErrorCodes.TutorialNotFound.Code;
                problemDetails.Extensions["tutorialId"] = tutorialEx.TutorialId;
                break;

            case NotImplementedException:
                problemDetails.Status = (int)HttpStatusCode.NotImplemented;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.2";
                problemDetails.Title = "Not Implemented";
                problemDetails.Detail = exception.Message;
                break;

            default:
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                problemDetails.Title = "An error occurred while processing your request.";
                problemDetails.Detail = exception.Message;
                break;
        }

        return problemDetails;
    }
}
