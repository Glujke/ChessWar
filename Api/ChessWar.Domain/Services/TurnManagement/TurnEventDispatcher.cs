using ChessWar.Domain.Entities;
using ChessWar.Domain.Events;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ChessWar.Domain.Services.TurnManagement;

/// <summary>
/// Диспетчер событий ходов
/// </summary>
public class TurnEventDispatcher : ITurnEventDispatcher
{
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ILogger<TurnEventDispatcher> _logger;

    public TurnEventDispatcher(IDomainEventDispatcher eventDispatcher, ILogger<TurnEventDispatcher> logger)
    {
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DispatchTurnStartedEventAsync(GameSession session, Turn turn)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));

        try
        {
            _logger.LogInformation("Turn {TurnNumber} started for participant {ParticipantId}", 
                turn.Number, turn.ActiveParticipant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching turn started event");
        }
    }

    public async Task DispatchTurnEndedEventAsync(GameSession session, Turn turn)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));

        try
        {
            _logger.LogInformation("Turn {TurnNumber} ended for participant {ParticipantId}", 
                turn.Number, turn.ActiveParticipant.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching turn ended event");
        }
    }

    public async Task DispatchPieceMovedEventAsync(GameSession session, Piece piece, Position fromPosition, Position toPosition)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        try
        {
            _logger.LogInformation("Piece {PieceId} moved from ({FromX},{FromY}) to ({ToX},{ToY})", 
                piece.Id, fromPosition.X, fromPosition.Y, toPosition.X, toPosition.Y);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching piece moved event");
        }
    }

    public async Task DispatchPieceAttackedEventAsync(GameSession session, Piece attacker, Piece target, int damage)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));
        if (target == null) throw new ArgumentNullException(nameof(target));

        try
        {
            _logger.LogInformation("Piece {AttackerId} attacked piece {TargetId} for {Damage} damage", 
                attacker.Id, target.Id, damage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dispatching piece attacked event");
        }
    }
}
