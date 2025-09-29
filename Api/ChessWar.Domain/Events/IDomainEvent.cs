namespace ChessWar.Domain.Events;

/// <summary>
/// Базовый интерфейс для доменных событий
/// </summary>
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}
