namespace ChessWar.Domain.Events.Handlers;

/// <summary>
/// Обработчик автоматического перемещения при убийстве фигуры
/// </summary>
public class PositionSwapHandler : IDomainEventHandler<PieceKilledEvent>
{
    public void Handle(PieceKilledEvent domainEvent)
    {
        domainEvent.Killer.Position = domainEvent.Victim.Position;
    }
}
