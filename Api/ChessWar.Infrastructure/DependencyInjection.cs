using Microsoft.Extensions.Logging;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Infrastructure.Repositories;
using ChessWar.Infrastructure.Services;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Services.AI;
using Microsoft.Extensions.DependencyInjection;
using ChessWar.Domain.Events;
using ChessWar.Domain.Events.Handlers;

namespace ChessWar.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IPieceRepository, PieceRepository>();
        services.AddScoped<IBalanceVersionRepository, BalanceVersionRepository>();
        services.AddScoped<IBalancePayloadRepository, BalancePayloadRepository>();
        services.AddScoped<IGameSessionRepository, GameSessionRepository>();
        services.AddScoped<IGameModeRepository, GameModeRepository>();
        
        services.AddScoped<IAttackRulesService, AttackRulesService>();
        services.AddScoped<IAbilityTargetProvider, AbilityTargetProvider>();
        services.AddScoped<IAbilityTargetService, AbilityTargetService>();
        services.AddScoped<IMovementRulesService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<MovementRulesService>>();
            return new MovementRulesService(logger);
        });
        services.AddScoped<IEvolutionService, EvolutionService>();
        services.AddScoped<IAbilityService, AbilityService>();

        services.AddScoped<IGameStateService, GameStateService>();
        services.AddScoped<IPieceFactory, PieceFactory>();
        services.AddScoped<IPieceDomainService, PieceDomainService>();
        
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<IDomainEventHandler<PieceKilledEvent>, ExperienceAwardHandler>();
        services.AddScoped<IDomainEventHandler<PieceKilledEvent>, BoardCleanupHandler>();
        services.AddScoped<IDomainEventHandler<PieceKilledEvent>, PositionSwapHandler>();
        
        
        services.AddScoped<IBalanceConfigProvider, BalanceConfigProvider>();
        services.AddScoped<PieceConfigService>();
        
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPieceConfigService, CachedPieceConfigService>();
        
        
        services.AddScoped<IGameStateEvaluator, GameStateEvaluator>();
        services.AddScoped<IProbabilityMatrix, ChessWarProbabilityMatrix>();
        services.AddScoped<IAIDifficultyLevel, ChessWar.Domain.Services.AI.AIDifficultyProvider>();
        
        
        services.AddScoped<IActionGenerator, ChessWar.Domain.Services.AI.ActionGenerator>();
        services.AddScoped<IActionSelector, ChessWar.Domain.Services.AI.ActionSelector>();
        services.AddScoped<IActionExecutor, ChessWar.Domain.Services.AI.ActionExecutor>();
        services.AddScoped<IAIService, ChessWar.Domain.Services.AI.AIService>();
        
        return services;
    }
}
