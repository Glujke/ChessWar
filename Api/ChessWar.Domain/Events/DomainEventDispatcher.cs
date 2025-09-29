using Microsoft.Extensions.DependencyInjection;

namespace ChessWar.Domain.Events;

/// <summary>
/// Простая реализация диспетчера доменных событий
/// </summary>
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;
    private readonly List<IDomainEvent> _events = new();

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public void Publish<T>(T domainEvent) where T : IDomainEvent
    {
        _events.Add(domainEvent);
        Console.WriteLine($"[DomainEventDispatcher] Published event: {domainEvent.GetType().Name}");
    }

    public void PublishAll()
    {
        Console.WriteLine($"[DomainEventDispatcher] Publishing {_events.Count} events");
        foreach (var domainEvent in _events)
        {
            var method = typeof(DomainEventDispatcher).GetMethod("PublishEvent", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var genericMethod = method!.MakeGenericMethod(domainEvent.GetType());
            genericMethod.Invoke(this, new object[] { domainEvent });
            Console.WriteLine($"[DomainEventDispatcher] Processed event: {domainEvent.GetType().Name}");
        }
        _events.Clear();
    }

    private void PublishEvent<T>(T domainEvent) where T : IDomainEvent
    {
        var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(typeof(T));
        var handlers = _serviceProvider.GetServices(handlerType);

        Console.WriteLine($"[DomainEventDispatcher] Found {handlers.Count()} handlers for {typeof(T).Name}");
        foreach (var handler in handlers)
        {
            if (handler is IDomainEventHandler<T> typedHandler)
            {
                Console.WriteLine($"[DomainEventDispatcher] Calling handler: {handler.GetType().Name}");
                typedHandler.Handle(domainEvent);
            }
        }
    }
}
