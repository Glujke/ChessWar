namespace ChessWar.Application.Interfaces.Configuration;

/// <summary>
/// Абстракция для отправки уведомлений через SignalR
/// Позволяет Application слою не зависеть от конкретной реализации SignalR
/// </summary>
public interface IGameHubClient
{
    /// <summary>
    /// Отправляет уведомление группе клиентов
    /// </summary>
    Task SendToGroupAsync(string groupName, string method, object data, CancellationToken cancellationToken = default);
}
