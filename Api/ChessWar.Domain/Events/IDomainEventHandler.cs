namespace ChessWar.Domain.Events;

/// <summary>
/// Интерфейс для обработчиков доменных событий
/// </summary>
/// <typeparam name="T">Тип доменного события</typeparam>
public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    /// <summary>
    /// Обрабатывает доменное событие
    /// </summary>
    void Handle(T domainEvent);
}
