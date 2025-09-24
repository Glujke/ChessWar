using ChessWar.Domain.Entities;

namespace ChessWar.Domain.Events;

/// <summary>
/// Событие убийства фигуры
/// </summary>
public class PieceKilledEvent : IDomainEvent
{
    public Piece Killer { get; }
    public Piece Victim { get; }
    public DateTime OccurredAt { get; }

    public PieceKilledEvent(Piece killer, Piece victim)
    {
        Killer = killer ?? throw new ArgumentNullException(nameof(killer));
        Victim = victim ?? throw new ArgumentNullException(nameof(victim));
        OccurredAt = DateTime.UtcNow;
    }
}
