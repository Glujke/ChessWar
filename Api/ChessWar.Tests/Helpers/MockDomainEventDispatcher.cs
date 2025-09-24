using ChessWar.Domain.Events;

namespace ChessWar.Tests.Helpers;

/// <summary>
/// Мок для IDomainEventDispatcher для тестов
/// </summary>
public class MockDomainEventDispatcher : IDomainEventDispatcher
{
    public void Publish<T>(T domainEvent) where T : IDomainEvent
    {
        // В тестах просто игнорируем события
    }

    public void PublishAll()
    {
        // В тестах просто игнорируем события
    }
}
