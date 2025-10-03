using Microsoft.AspNetCore.Mvc;

namespace ChessWar.Api.Controllers;

/// <summary>
/// Базовый контроллер приложения с общими возможностями логирования
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Экземпляр логгера для производных контроллеров
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Создаёт экземпляр базового контроллера
    /// </summary>
    /// <param name="logger">Логгер контроллера</param>
    protected BaseController(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Записывает информационное сообщение в лог
    /// </summary>
    protected void LogInformation(string message, params object[] args)
    {
        Logger.LogInformation(message, args);
    }

    /// <summary>
    /// Записывает предупреждение в лог
    /// </summary>
    protected void LogWarning(string message, params object[] args)
    {
        Logger.LogWarning(message, args);
    }

    /// <summary>
    /// Записывает ошибку в лог
    /// </summary>
    protected void LogError(Exception exception, string message, params object[] args)
    {
        Logger.LogError(exception, message, args);
    }

    /// <summary>
    /// Записывает отладочное сообщение в лог
    /// </summary>
    protected void LogDebug(string message, params object[] args)
    {
        Logger.LogDebug(message, args);
    }
}
