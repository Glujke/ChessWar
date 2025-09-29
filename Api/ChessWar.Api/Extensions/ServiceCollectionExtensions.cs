using ChessWar.Api.Middleware;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Interfaces.Tutorial;

namespace ChessWar.Api.Extensions;

/// <summary>
/// Расширения для настройки сервисов
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет ProblemDetails middleware
    /// </summary>
    public static IApplicationBuilder UseProblemDetails(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ProblemDetailsMiddleware>();
    }

    /// <summary>
    /// Добавляет сервисы для режимов игры
    /// </summary>
    public static IServiceCollection AddGameModeServices(this IServiceCollection services)
    {
        services.AddScoped<IGameHubClient, ChessWar.Api.Services.SignalRGameHubClient>();
        
        services.AddScoped<ITutorialService, ChessWar.Application.Services.Tutorial.TutorialService>();
        services.AddScoped<ITutorialHintService, ChessWar.Application.Services.Tutorial.TutorialHintService>();
        
        return services;
    }
}
