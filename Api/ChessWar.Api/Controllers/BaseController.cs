using Microsoft.AspNetCore.Mvc;

namespace ChessWar.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected readonly ILogger Logger;

    protected BaseController(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected void LogInformation(string message, params object[] args)
    {
        Logger.LogInformation(message, args);
    }

    protected void LogWarning(string message, params object[] args)
    {
        Logger.LogWarning(message, args);
    }

    protected void LogError(Exception exception, string message, params object[] args)
    {
        Logger.LogError(exception, message, args);
    }

    protected void LogDebug(string message, params object[] args)
    {
        Logger.LogDebug(message, args);
    }
}
