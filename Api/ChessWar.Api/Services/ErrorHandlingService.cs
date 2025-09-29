using Microsoft.AspNetCore.Mvc;

namespace ChessWar.Api.Services;

/// <summary>
/// Сервис для стандартизированной обработки ошибок по REST API
/// </summary>
public interface IErrorHandlingService
{
    /// <summary>
    /// Создает стандартизированный ответ об ошибке валидации (400 Bad Request)
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <param name="details">Дополнительные детали</param>
    /// <returns>ActionResult с ошибкой валидации</returns>
    ActionResult CreateValidationError(string message, string? details = null);

    /// <summary>
    /// Создает стандартизированный ответ об ошибке "не найдено" (404 Not Found)
    /// </summary>
    /// <param name="resource">Тип ресурса</param>
    /// <param name="identifier">Идентификатор ресурса</param>
    /// <returns>ActionResult с ошибкой "не найдено"</returns>
    ActionResult CreateNotFoundError(string resource, string identifier);

    /// <summary>
    /// Создает стандартизированный ответ об ошибке конфликта (409 Conflict)
    /// </summary>
    /// <param name="message">Сообщение о конфликте</param>
    /// <param name="details">Дополнительные детали</param>
    /// <returns>ActionResult с ошибкой конфликта</returns>
    ActionResult CreateConflictError(string message, string? details = null);

    /// <summary>
    /// Создает стандартизированный ответ о нарушении правил игры (400 Bad Request)
    /// </summary>
    /// <param name="rule">Нарушенное правило</param>
    /// <param name="details">Детали нарушения</param>
    /// <returns>ActionResult с ошибкой нарушения правил</returns>
    ActionResult CreateRuleViolationError(string rule, string? details = null);

    /// <summary>
    /// Создает стандартизированный ответ о внутренней ошибке сервера (500 Internal Server Error)
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
    /// <returns>ActionResult с внутренней ошибкой</returns>
    ActionResult CreateInternalServerError(string message);
}

/// <summary>
/// Реализация сервиса обработки ошибок
/// </summary>
public class ErrorHandlingService : IErrorHandlingService
{
    public ActionResult CreateValidationError(string message, string? details = null)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Validation Error",
            Status = 400,
            Detail = message
        };

        if (!string.IsNullOrEmpty(details))
        {
            problemDetails.Extensions["details"] = details;
        }

        return new BadRequestObjectResult(problemDetails);
    }

    public ActionResult CreateNotFoundError(string resource, string identifier)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = "Resource Not Found",
            Status = 404,
            Detail = $"{resource} with identifier '{identifier}' not found"
        };

        return new NotFoundObjectResult(problemDetails);
    }

    public ActionResult CreateConflictError(string message, string? details = null)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Title = "Conflict",
            Status = 409,
            Detail = message
        };

        if (!string.IsNullOrEmpty(details))
        {
            problemDetails.Extensions["details"] = details;
        }

        return new ConflictObjectResult(problemDetails);
    }

    public ActionResult CreateRuleViolationError(string rule, string? details = null)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "Game Rule Violation",
            Status = 400,
            Detail = $"Rule violation: {rule}"
        };

        if (!string.IsNullOrEmpty(details))
        {
            problemDetails.Extensions["details"] = details;
        }

        return new BadRequestObjectResult(problemDetails);
    }

    public ActionResult CreateInternalServerError(string message)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            Title = "Internal Server Error",
            Status = 500,
            Detail = message
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = 500
        };
    }
}
