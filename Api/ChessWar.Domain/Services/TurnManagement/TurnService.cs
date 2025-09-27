using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Events;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace ChessWar.Domain.Services.TurnManagement;

/// <summary>
/// Координатор ходов - управляет выполнением действий в игре
/// </summary>
public class TurnService : ITurnService
{
    private readonly IMovementRulesService _movementRulesService;
    private readonly IAttackRulesService _attackRulesService;
    private readonly IEvolutionService _evolutionService;
    private readonly IBalanceConfigProvider _configProvider;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly IPieceDomainService _pieceDomainService;
    private readonly ILogger<TurnService> _logger;

    public TurnService(
        IMovementRulesService movementRulesService,
        IAttackRulesService attackRulesService,
        IEvolutionService evolutionService,
        IBalanceConfigProvider configProvider,
        IDomainEventDispatcher eventDispatcher,
        IPieceDomainService pieceDomainService,
        ILogger<TurnService> logger)
    {
        _movementRulesService = movementRulesService ?? throw new ArgumentNullException(nameof(movementRulesService));
        _attackRulesService = attackRulesService ?? throw new ArgumentNullException(nameof(attackRulesService));
        _evolutionService = evolutionService ?? throw new ArgumentNullException(nameof(evolutionService));
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _pieceDomainService = pieceDomainService ?? throw new ArgumentNullException(nameof(pieceDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Turn StartTurn(GameSession gameSession, Participant activeParticipant)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (activeParticipant == null)
            throw new ArgumentNullException(nameof(activeParticipant));

        var turnNumber = gameSession.CurrentTurn?.Number + 1 ?? 1;
        var turn = new Turn(turnNumber, activeParticipant);
        
        _logger.LogInformation("Started turn {TurnNumber} for participant {ParticipantId}", 
            turnNumber, activeParticipant.Id);

        return turn;
    }

    public bool ExecuteMove(GameSession session, Turn turn, Piece piece, Position targetPosition)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        try
        {
            if (piece.Owner != turn.ActiveParticipant)
            {
                _logger.LogWarning("Piece {PieceId} does not belong to active participant {ParticipantId}", piece.Id, turn.ActiveParticipant.Id);
                return false;
            }

            if (!_movementRulesService.CanMoveTo(piece, targetPosition, session.GetBoard().Pieces.ToList()))
            {
                _logger.LogWarning("Piece {PieceId} cannot move to ({X},{Y}) based on rules", piece.Id, targetPosition.X, targetPosition.Y);
                return false;
            }

            _pieceDomainService.MoveTo(piece, targetPosition);
            
            var moveCost = _configProvider.GetActive().PlayerMana.MovementCosts[piece.Type.ToString()];
            turn.SpendMP(moveCost);
            piece.Owner.Spend(moveCost);
            
            turn.AddAction(new TurnAction("Move", piece.Id.ToString(), targetPosition));

            _logger.LogInformation("Piece {PieceId} moved to ({X},{Y}). Remaining MP: {MP}", piece.Id, targetPosition.X, targetPosition.Y, turn.RemainingMP);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing move for piece {PieceId}", piece.Id);
            return false;
        }
    }

    public bool ExecuteAttack(GameSession session, Turn turn, Piece attacker, Position targetPosition)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));

        try
        {
            _logger.LogInformation("[DEBUG] ExecuteAttack: attacker={AttackerId}, target=({X},{Y})", attacker.Id, targetPosition.X, targetPosition.Y);
            
            if (attacker.Owner != turn.ActiveParticipant)
            {
                _logger.LogWarning("Attacker piece {AttackerId} does not belong to active participant {ParticipantId}", attacker.Id, turn.ActiveParticipant.Id);
                return false;
            }

            var targetPiece = session.GetBoard().GetPieceAt(targetPosition);
            _logger.LogInformation("[DEBUG] Target piece found: {TargetPieceId}", targetPiece?.Id);
            if (targetPiece == null)
            {
                _logger.LogWarning("No piece found at target position ({X},{Y})", targetPosition.X, targetPosition.Y);
                return false;
            }

            var canAttack = _attackRulesService.CanAttack(attacker, targetPosition, session.GetBoard().Pieces.ToList());
            _logger.LogInformation("[DEBUG] CanAttack result: {CanAttack}", canAttack);
            if (!canAttack)
            {
                _logger.LogWarning("Attacker {AttackerId} cannot attack target at ({X},{Y}) based on rules", attacker.Id, targetPosition.X, targetPosition.Y);
                return false;
            }

            _pieceDomainService.TakeDamage(targetPiece, attacker.Attack);
            var attackCost = _configProvider.GetActive().PlayerMana.AttackCost;
            turn.SpendMP(attackCost);
            attacker.Owner.Spend(attackCost);
            
            if (_pieceDomainService.IsDead(targetPiece))
            {
                session.GetBoard().RemovePiece(targetPiece);
                session.GetBoard().MovePiece(attacker, targetPosition);
                
                _logger.LogInformation("Target {TargetPieceId} killed, attacker {AttackerId} moved to position ({X},{Y})", 
                    targetPiece.Id, attacker.Id, targetPosition.X, targetPosition.Y);
            }
            
            turn.AddAction(new TurnAction("Attack", attacker.Id.ToString(), targetPosition));

            _logger.LogInformation("Attacker {AttackerId} attacked target {TargetPieceId}. Turn MP: {TurnMP}, Target HP: {TargetHP}",
                attacker.Id, targetPiece.Id, turn.RemainingMP, targetPiece.HP);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing attack for piece {AttackerId}", attacker.Id);
            return false;
        }
    }

    public List<Position> GetAvailableMoves(GameSession session, Turn turn, Piece piece)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        try
        {
            return _movementRulesService.GetAvailableMoves(piece, session.GetBoard().Pieces.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available moves for piece {PieceId}", piece.Id);
            return new List<Position>();
        }
    }

    public List<Position> GetAvailableAttacks(Turn turn, Piece attacker)
    {
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        if (attacker == null) throw new ArgumentNullException(nameof(attacker));

        try
        {
            return _attackRulesService.GetAttackablePositions(attacker, new List<Piece>()).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available attacks for piece {AttackerId}", attacker.Id);
            return new List<Position>();
        }
    }

    public bool CanEvolve(GameSession session, Turn turn, Piece piece)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        try
        {
            return piece.CanEvolve && turn.CanAfford(5);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking evolution possibility for piece {PieceId}", piece.Id);
            return false;
        }
    }

    public bool ExecuteEvolution(GameSession session, Turn turn, Piece piece)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        if (piece == null) throw new ArgumentNullException(nameof(piece));

        try
        {
            if (!CanEvolve(session, turn, piece))
            {
                _logger.LogWarning("Piece {PieceId} cannot evolve", piece.Id);
                return false;
            }

            _evolutionService.EvolvePiece(piece, piece.Type);
            
            turn.AddAction(new TurnAction("Evolve", piece.Id.ToString(), piece.Position));

            _logger.LogInformation("Piece {PieceId} evolved successfully", piece.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing evolution for piece {PieceId}", piece.Id);
            return false;
        }
    }

    public void EndTurn(Turn turn)
    {
        if (turn == null) throw new ArgumentNullException(nameof(turn));
        
        _logger.LogInformation("Ending turn {TurnNumber}", turn.Number);
        
        foreach (var piece in turn.ActiveParticipant.Pieces)
        {
            _pieceDomainService.TickCooldowns(piece);
        }
    }

    private int CalculateDamage(Piece attacker, Piece target)
    {
        return attacker.Attack;
    }
    

}