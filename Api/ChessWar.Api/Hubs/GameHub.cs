using Microsoft.AspNetCore.SignalR;
using ChessWar.Application.Interfaces.Configuration;

namespace ChessWar.Api.Hubs;

/// <summary>
/// SignalR Hub для уведомлений о игровых событиях
/// </summary>
public class GameHub : Hub, IGameHub
{
    private readonly ILogger<GameHub> _logger;

    public GameHub(ILogger<GameHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Клиент подключается к игре
    /// </summary>
    public async Task JoinGame(string gameId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        _logger.LogInformation("Player {ConnectionId} joined game {GameId}", 
            Context.ConnectionId, gameId);
    }

    /// <summary>
    /// Клиент отключается от игры
    /// </summary>
    public async Task LeaveGame(string gameId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        _logger.LogInformation("Player {ConnectionId} left game {GameId}", 
            Context.ConnectionId, gameId);
    }

    /// <summary>
    /// Клиент подключается к SignalR
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        
        var sessionId = Context.GetHttpContext().Request.Query["sessionId"];
        if (!string.IsNullOrEmpty(sessionId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            _logger.LogInformation("Client {ConnectionId} automatically joined group {SessionId}", 
                Context.ConnectionId, sessionId);
        }
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Клиент отключается от SignalR
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
