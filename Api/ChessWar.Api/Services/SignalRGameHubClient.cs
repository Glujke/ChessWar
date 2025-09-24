using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChessWar.Api.Services;

/// <summary>
/// Реализация IGameHubClient через SignalR
/// </summary>
public class SignalRGameHubClient : IGameHubClient
{
    private readonly IHubContext<GameHub> _hubContext;

    public SignalRGameHubClient(IHubContext<GameHub> hubContext)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
    }

    public async Task SendToGroupAsync(string groupName, string method, object data, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group(groupName).SendAsync(method, data, cancellationToken);
    }
}
