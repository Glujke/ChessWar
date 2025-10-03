using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ChessWar.Persistence.Sqlite;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Tests.Unit;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Interfaces.GameManagement;
using System.IO;

namespace ChessWar.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration(cfg =>
        {
            var dict = new Dictionary<string, string?>
            {
                ["Cors:AllowedOrigins:0"] = "http://localhost"
            };
            cfg.AddInMemoryCollection(dict!);
        });
        Directory.CreateDirectory("Logs");
        Directory.CreateDirectory("App_Data");
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChessWarDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ChessWarDbContext));
            if (contextDescriptor != null)
                services.Remove(contextDescriptor);

            var sqliteServices = services.Where(s => s.ServiceType.FullName?.Contains("Sqlite") == true).ToList();
            foreach (var service in sqliteServices)
            {
                services.Remove(service);
            }

            var configProviderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBalanceConfigProvider));
            if (configProviderDescriptor != null)
                services.Remove(configProviderDescriptor);

            services.AddDbContext<ChessWarDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddScoped<IBalanceConfigProvider>(_ => _TestConfig.CreateProvider());

            var queueImpl = services.Where(s => s.ServiceType.FullName == "ChessWar.Application.Services.Board.TurnProcessingBackgroundService").ToList();
            foreach (var service in queueImpl)
                services.Remove(service);

            var queueAbstraction = services.Where(s => s.ServiceType.FullName == "ChessWar.Application.Interfaces.Board.ITurnProcessingQueue").ToList();
            foreach (var service in queueAbstraction)
                services.Remove(service);

            var notificationBatcherInterface = services.Where(s => s.ServiceType.FullName == "ChessWar.Application.Interfaces.Board.INotificationBatcher").ToList();
            foreach (var service in notificationBatcherInterface)
                services.Remove(service);

            // Remove any INotificationBatcher registrations (by type), regardless of how they were registered
            var toRemove = services.Where(d => d.ServiceType == typeof(ChessWar.Application.Interfaces.Board.INotificationBatcher)).ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            // Ensure stub registration (scoped to be safe with scoped deps, though it is no-op)
            services.AddScoped<ChessWar.Application.Interfaces.Board.INotificationBatcher, TestNotificationBatcherStub>();

            // Remove any INotificationDispatcher registrations and replace with a scoped stub
            var dispatcherDescriptors = services.Where(d => d.ServiceType == typeof(ChessWar.Application.Interfaces.Board.INotificationDispatcher)).ToList();
            foreach (var d in dispatcherDescriptors)
            {
                services.Remove(d);
            }
            services.AddScoped<ChessWar.Application.Interfaces.Board.INotificationDispatcher, TestNotificationDispatcherStub>();

            var hubClientDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IGameHubClient));
            if (hubClientDescriptor != null) services.Remove(hubClientDescriptor);
            services.AddScoped<IGameHubClient, TestGameHubClientStub>();

            var notifServiceDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IGameNotificationService));
            if (notifServiceDescriptor != null) services.Remove(notifServiceDescriptor);
            services.AddScoped<IGameNotificationService, TestGameNotificationServiceStub>();

            services.AddSingleton<ChessWar.Application.Interfaces.Board.ITurnProcessingQueue>(sp => new TestTurnProcessingQueueStub(sp));

        });

        // Ensure final override after the app registers all services
        builder.ConfigureTestServices(services =>
        {
            var batchers = services.Where(d => d.ServiceType == typeof(ChessWar.Application.Interfaces.Board.INotificationBatcher)).ToList();
            foreach (var d in batchers) services.Remove(d);
            services.AddSingleton<ChessWar.Application.Interfaces.Board.INotificationBatcher, TestNotificationBatcherStub>();

            var dispatchers = services.Where(d => d.ServiceType == typeof(ChessWar.Application.Interfaces.Board.INotificationDispatcher)).ToList();
            foreach (var d in dispatchers) services.Remove(d);
            services.AddSingleton<ChessWar.Application.Interfaces.Board.INotificationDispatcher, TestNotificationDispatcherStub>();
        });

        // Disable scope validation to avoid singleton->scoped checks in test host
        builder.UseDefaultServiceProvider((context, options) =>
        {
            options.ValidateScopes = false;
            options.ValidateOnBuild = false;
        });

    }


    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChessWarDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}

internal sealed class TestTurnProcessingQueueStub : ChessWar.Application.Interfaces.Board.ITurnProcessingQueue
{
    private readonly IServiceProvider _serviceProvider;

    public TestTurnProcessingQueueStub(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> EnqueueTurnAsync(ChessWar.Application.Services.Board.TurnRequest request)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var turnProcessor = scope.ServiceProvider.GetRequiredService<ChessWar.Application.Interfaces.Board.ITurnProcessor>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestTurnProcessingQueueStub>>();
            logger.LogInformation("TestTurnProcessingQueueStub: Processing turn for session {SessionId}, actions count: {ActionsCount}",
                request.GameSession.Id, request.GameSession.GetCurrentTurn().Actions?.Count ?? 0);
            await turnProcessor.ProcessTurnPhaseAsync(request.GameSession, CancellationToken.None);
            logger.LogInformation("TestTurnProcessingQueueStub: Turn processing completed successfully for session {SessionId}", request.GameSession.Id);
            return true;
        }
        catch (Exception ex)
        {
            using var scope = _serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestTurnProcessingQueueStub>>();
            logger.LogError(ex, "TestTurnProcessingQueueStub: Turn processing failed for session {SessionId}: {Message}",
                request.GameSession.Id, ex.Message);
            return false;
        }
    }
}

internal sealed class TestGameHubClientStub : IGameHubClient
{
    public Task SendToGroupAsync(string groupName, string method, object data, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

internal sealed class TestGameNotificationServiceStub : IGameNotificationService
{
    public Task NotifyAiMoveAsync(Guid sessionId, object moveData, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyGameEndedAsync(Guid sessionId, string result, string message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyErrorAsync(Guid sessionId, string error, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyTutorialAdvancedAsync(Guid tutorialId, string stage, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyPieceEvolvedAsync(Guid sessionId, string pieceId, string newType, int x, int y, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyAITurnInProgressAsync(Guid sessionId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyAITurnCompletedAsync(Guid sessionId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyTurnStartedAsync(Guid sessionId, string participantType, int turnNumber, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task NotifyTurnEndedAsync(Guid sessionId, string participantType, int turnNumber, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class TestNotificationBatcherStub : ChessWar.Application.Interfaces.Board.INotificationBatcher
{
    public void AddNotification(ChessWar.Application.Services.Board.NotificationBatchItem item) { }
    public void AddTurnStartedNotification(Guid sessionId, string participantType, int turnNumber) { }
    public void AddTurnEndedNotification(Guid sessionId, string participantType, int turnNumber) { }
    public void AddGameEndedNotification(Guid sessionId, string result, string message) { }
    public Task BatchNotificationAsync<T>(Guid sessionId, string method, T data, CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task FlushAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}

internal sealed class TestNotificationDispatcherStub : ChessWar.Application.Interfaces.Board.INotificationDispatcher
{
    public void DispatchTurnEnded(Guid sessionId, string participantType, int turnNumber) { }
    public void DispatchTurnStarted(Guid sessionId, string participantType, int turnNumber) { }
    public void DispatchAITurnInProgress(Guid sessionId) { }
    public void DispatchAITurnCompleted(Guid sessionId) { }
    public void DispatchAiMove(Guid sessionId, object moveData) { }
    public void DispatchGameEnded(Guid sessionId, string result, string message) { }
    public void DispatchPieceEvolved(Guid sessionId, string pieceId, string newType, int x, int y) { }
    public void DispatchError(Guid sessionId, string error) { }
}
