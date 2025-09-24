namespace ChessWar.Domain.Events;

/// <summary>
/// Интерфейс для диспетчера доменных событий
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Публикует доменное событие
    /// </summary>
    void Publish<T>(T domainEvent) where T : IDomainEvent;
    
    /// <summary>
    /// Публикует все накопленные события
    /// </summary>
    void PublishAll();
}
