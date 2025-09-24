namespace ChessWar.Domain.Events.Handlers;

/// <summary>
/// Обработчик очистки доски после убийства фигуры
/// </summary>
public class BoardCleanupHandler : IDomainEventHandler<PieceKilledEvent>
{
    public void Handle(PieceKilledEvent domainEvent)
    {
        domainEvent.Victim.HP = 0;
    }
}
