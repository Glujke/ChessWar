using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ChessWar.Application.Interfaces.GameManagement;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Application.Interfaces.AI;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Commands;
using ChessWar.Application.Services.GameManagement;
using ChessWar.Application.Services.Board;
using ChessWar.Application.Services.Pieces;
using ChessWar.Application.Services.AI;
using ChessWar.Application.Services.Tutorial;
using ChessWar.Application.Services.Configuration;
using ChessWar.Application.Services;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.Events;

namespace ChessWar.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPieceService, PieceService>();
        services.AddScoped<IBoardService, BoardService>();
        services.AddScoped<IConfigService, ConfigService>();
        
        services.AddScoped<IGameSessionManagementService, GameSessionManagementService>();
        services.AddScoped<IGameSessionFactory, GameSessionFactory>();
        services.AddScoped<IGameManagementService, GameManagementService>();
        
        services.AddScoped<IActionExecutionService, ActionExecutionService>();
        services.AddScoped<IActionQueryService, ActionQueryService>();
        services.AddScoped<ITurnOrchestrator, TurnOrchestrator>();
        services.AddScoped<ITurnCompletionService, TurnCompletionService>();
        services.AddScoped<ITurnExecutionService, TurnExecutionService>();
        
        
        services.AddScoped<IAttackApplicationService, AttackApplicationService>();
        services.AddScoped<IPieceSearchService, PieceSearchService>();
        
        services.AddScoped<IAITurnService, AITurnService>();
        
        services.AddScoped<IScenarioService, ScenarioTutorialService>();
        
        services.AddScoped<IStatsService, StatsService>();
        services.AddScoped<IGameModeService, ChessWar.Application.Services.Configuration.GameModeService>();
        services.AddScoped<IBattlePresetService, BattlePresetService>();
        services.AddScoped<IPlayerManagementService, PlayerManagementService>();
        
        services.AddScoped<IGameNotificationService, GameNotificationService>();
        
        services.AddSingleton<ChessWar.Domain.Interfaces.Configuration.IPieceIdGenerator, PieceIdGenerator>();
        
        services.AddScoped<ICommandFactory, CommandFactory>();
        
        services.AddScoped<ChessWar.Application.Services.Common.IPieceValidationService, ChessWar.Application.Services.Common.PieceValidationService>();
        services.AddScoped<ChessWar.Application.Services.Common.IBoardContextService, ChessWar.Application.Services.Common.BoardContextService>();
        services.AddScoped<ChessWar.Application.Interfaces.GameModes.IGameModeStrategyFactory, ChessWar.Application.Services.GameModes.GameModeStrategyFactory>();
        
        services.AddScoped<ITurnService>(provider =>
        {
            var movementRulesService = provider.GetRequiredService<IMovementRulesService>();
            var attackRulesService = provider.GetRequiredService<IAttackRulesService>();
            var evolutionService = provider.GetRequiredService<IEvolutionService>();
            var configProvider = provider.GetRequiredService<IBalanceConfigProvider>();
            var eventDispatcher = provider.GetRequiredService<IDomainEventDispatcher>();
            var pieceDomainService = provider.GetRequiredService<IPieceDomainService>();
            var logger = provider.GetRequiredService<ILogger<TurnService>>();
            
            return new TurnService(movementRulesService, attackRulesService, evolutionService, configProvider, eventDispatcher, pieceDomainService, logger);
        });
        
        return services;
    }
}
