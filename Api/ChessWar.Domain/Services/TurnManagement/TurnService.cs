using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Events;
using Microsoft.Extensions.Logging;

namespace ChessWar.Domain.Services.TurnManagement;

/// <summary>
/// Сервис управления ходами в игре
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

    public Turn StartTurn(GameSession gameSession, Player activeParticipant)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (activeParticipant == null)
            throw new ArgumentNullException(nameof(activeParticipant));

        var turnNumber = gameSession.CurrentTurn?.Number + 1 ?? 1;
        var turn = new Turn(turnNumber, activeParticipant);

        return turn;
    }


    public bool ExecuteMove(GameSession gameSession, Turn turn, Piece piece, Position targetPosition)
    {
        _logger.LogDebug("[TurnService] ExecuteMove called for piece {PieceId} to ({X},{Y})", piece?.Id, targetPosition?.X, targetPosition?.Y);
        _logger.LogDebug("[TurnService] Turn active participant: {ActiveParticipantName} (ID: {ActiveParticipantId})", turn.ActiveParticipant.Name, turn.ActiveParticipant.Id);
        _logger.LogDebug("[TurnService] Piece owner: {PieceOwnerName} (ID: {PieceOwnerId})", piece.Owner?.Name, piece.Owner?.Id);
        
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        if (targetPosition == null)
            throw new ArgumentNullException(nameof(targetPosition));

        if (piece.AbilityCooldowns.GetValueOrDefault("__RoyalCommandGranted", 0) > 0)
        {
            piece.AbilityCooldowns["__RoyalCommandGranted"] = 0;
        }
        turn.SelectPiece(piece);

        var allPieces = gameSession.Board?.Pieces?.Where(p => p.IsAlive).ToList() ?? new List<Piece>();
        _logger.LogInformation("[TurnService] Checking movement rules for piece {PieceId} from ({FromX},{FromY}) to ({ToX},{ToY})", 
            piece.Id, piece.Position.X, piece.Position.Y, targetPosition.X, targetPosition.Y);
        _logger.LogInformation("[TurnService] Piece type: {PieceType}, Team: {Team}", piece.Type, piece.Team);
        _logger.LogInformation("[TurnService] Total pieces on board: {PiecesCount}", allPieces.Count);
        
        var targetPiece = allPieces.FirstOrDefault(p => p.Position == targetPosition);
        _logger.LogInformation("[TurnService] Target position occupied by: {TargetPieceInfo}", 
            targetPiece != null ? $"Piece {targetPiece.Id} (Team: {targetPiece.Team})" : "Empty");
        
        var canMove = _movementRulesService.CanMoveTo(piece, targetPosition, allPieces);
        _logger.LogInformation("[TurnService] CanMoveTo result: {CanMove}", canMove);
        
        if (canMove)
        {
            _logger.LogInformation("🎯 [REAL MOVE] {PieceType} {PieceId} from ({FromX},{FromY}) to ({ToX},{ToY}) by {PlayerName}", 
                piece.Type, piece.Id, piece.Position.X, piece.Position.Y, targetPosition.X, targetPosition.Y, piece.Owner?.Name);
        }
        
        if (!canMove)
        {
            var possibleMoves = _movementRulesService.GetAvailableMoves(piece, allPieces) ?? new List<Position>();
            if (possibleMoves.Any(p => p == targetPosition))
            {
                _logger.LogWarning("[TurnService] CanMoveTo returned false, but target present in GetAvailableMoves. Overriding to allow move.");
                canMove = true;
            }
            var targetAlly = allPieces.FirstOrDefault(p => p.Position == targetPosition && p.Team == piece.Team) != null;
            var targetEnemy = allPieces.FirstOrDefault(p => p.Position == targetPosition && p.Team != piece.Team) != null;
            _logger.LogWarning("[TurnService] Move denied. Piece={PieceId} {Type} From=({FromX},{FromY}) To=({ToX},{ToY}) TargetAlly={TargetAlly} TargetEnemy={TargetEnemy}",
                piece.Id, piece.Type, piece.Position.X, piece.Position.Y, targetPosition.X, targetPosition.Y, targetAlly, targetEnemy);
        }
        
        if (!canMove)
        {
            _logger.LogInformation("[TurnService] Movement denied by rules!");
            return false;
        }

        var config = _configProvider.GetActive();
        var pieceTypeName = piece.Type.ToString();
        var movementCost = config.PlayerMana.MovementCosts.GetValueOrDefault(pieceTypeName, 1);
        
        _logger.LogInformation("[TurnService] Movement cost for {PieceType}: {MovementCost}", pieceTypeName, movementCost);
        _logger.LogInformation("[TurnService] Turn RemainingMP: {RemainingMP}", turn.RemainingMP);
        _logger.LogInformation("[TurnService] Turn ActiveParticipant MP: {ActiveParticipantMP}", turn.ActiveParticipant.MP);
        _logger.LogInformation("[TurnService] About to check free action marker...");

        var hasFreeActionMarker = piece.AbilityCooldowns.GetValueOrDefault("__RoyalCommandFreeAction", 0) > 0;
        var isFreeThisAction = hasFreeActionMarker;
        _logger.LogInformation("[TurnService] Has free action marker: {HasFreeActionMarker}", hasFreeActionMarker);
        
        if (!isFreeThisAction)
        {
            var canAfford = turn.CanAfford(movementCost);
            _logger.LogInformation("[TurnService] Can afford movement: {CanAfford}", canAfford);
            if (!canAfford)
            {
                _logger.LogInformation("[TurnService] Movement denied - insufficient mana!");
                return false;
            }
        }

        gameSession.Board.MovePiece(piece, targetPosition);
        
        if (isFreeThisAction)
        {
            piece.AbilityCooldowns["__RoyalCommandFreeAction"] = 0;
        }
        else
        {
            turn.SpendMP(movementCost);
            turn.ActiveParticipant.Spend(movementCost);
        }
        
        var action = new TurnAction("Move", piece.Id.ToString(), targetPosition);
        turn.AddAction(action);
        
        turn.UpdateRemainingMP();

        return true;
    }


    public bool ExecuteAttack(GameSession gameSession, Turn turn, Piece attacker, Position targetPosition)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        if (attacker == null)
            throw new ArgumentNullException(nameof(attacker));

        if (targetPosition == null)
            throw new ArgumentNullException(nameof(targetPosition));

        if (attacker.AbilityCooldowns.GetValueOrDefault("__RoyalCommandGranted", 0) > 0)
        {
            attacker.AbilityCooldowns["__RoyalCommandGranted"] = 0;
        }
        turn.SelectPiece(attacker);

        var allPieces = new List<Piece>();
        allPieces.AddRange(gameSession.Player1.Pieces);
        allPieces.AddRange(gameSession.Player2.Pieces);
        
        var targetPiece = allPieces.FirstOrDefault(p => p.Position.Equals(targetPosition));
        if (targetPiece != null && !targetPiece.IsAlive)
        {
            return false;
        }
        
        var boardPieces = gameSession.Board.Pieces.Where(p => p.IsAlive).ToList();
        
        if (!_attackRulesService.CanAttack(attacker, targetPosition, boardPieces))
            return false;

        var config = _configProvider.GetActive();
        var attackCost = config.PlayerMana.AttackCost;

        if (targetPiece != null)
        {
            _logger.LogDebug("[TurnService.ExecuteAttack] Target found: {TargetType}, HP: {TargetHP}, Attack: {AttackerAttack}", 
                targetPiece.Type, targetPiece.HP, attacker.Attack);
            _pieceDomainService.TakeDamage(targetPiece, attacker.Attack);
            _logger.LogDebug("[TurnService.ExecuteAttack] After damage: HP: {TargetHP}, IsDead: {IsDead}", 
                targetPiece.HP, _pieceDomainService.IsDead(targetPiece));
            
            if (_pieceDomainService.IsDead(targetPiece))
            {
                _logger.LogDebug("[TurnService.ExecuteAttack] Target is DEAD! Publishing PieceKilledEvent");
                _eventDispatcher.Publish(new PieceKilledEvent(attacker, targetPiece));
                _eventDispatcher.PublishAll();
                gameSession.Board.MovePiece(attacker, targetPosition);
            }
            else
            {
                _logger.LogDebug("[TurnService.ExecuteAttack] Target is ALIVE, no event published");
            }
        }
        else
        {
            _logger.LogDebug("[TurnService.ExecuteAttack] No target piece found at position {TargetPosition}", targetPosition);
        }
        
        if (!turn.CanAfford(attackCost))
            return false;
        turn.SpendMP(attackCost);
        turn.ActiveParticipant.Spend(attackCost);
        
        var action = new TurnAction("Attack", attacker.Id.ToString(), targetPosition);
        turn.AddAction(action);
        
        turn.UpdateRemainingMP();
        
        return true;
    }

    public void EndTurn(Turn turn)
    {
        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        var config = _configProvider.GetActive();
        var activePlayer = turn.ActiveParticipant;
        
        if (config.PlayerMana.MandatoryAction && turn.Actions.Count == 0)
        {
            throw new InvalidOperationException("Нельзя завершить ход без выполнения хотя бы одного действия");
        }
        
        
        foreach (var piece in activePlayer.Pieces)
        {
            _pieceDomainService.TickCooldowns(piece);
            var hasBuffMarker = piece.AbilityCooldowns.GetValueOrDefault("__FortressBuff", 0) > 0;
            if (!hasBuffMarker && piece.HP > _pieceDomainService.GetMaxHP(piece.Type))
            {
                piece.HP = _pieceDomainService.GetMaxHP(piece.Type);
            }
            if (piece.Type == ChessWar.Domain.Enums.PieceType.King && config.Ai.KingAura != null)
            {
                ApplyKingAura(piece, activePlayer.Pieces, config.Ai.KingAura);
            }
        }
        
        _eventDispatcher.PublishAll();
    }

    private void ApplyKingAura(Piece king, List<Piece> allies, ChessWar.Domain.Entities.Config.KingAuraConfig auraConfig)
    {
        foreach (var ally in allies)
        {
            if (ally == king) continue;
            var d = Math.Max(Math.Abs(ally.Position.X - king.Position.X), Math.Abs(ally.Position.Y - king.Position.Y));
            var inRange = d <= auraConfig.Radius;
            var hasAura = ally.AbilityCooldowns.GetValueOrDefault("__AuraBuff", 0) > 0;

            if (inRange && !hasAura)
            {
                ally.ATK += auraConfig.AtkBonus;
                _pieceDomainService.SetAbilityCooldown(ally, "__AuraBuff", 2); 
            }
            else if (!inRange && hasAura)
            {
                ally.ATK = Math.Max(0, ally.ATK - auraConfig.AtkBonus);
                ally.AbilityCooldowns["__AuraBuff"] = 0;
            }
            else if (inRange && hasAura)
            {
                _pieceDomainService.SetAbilityCooldown(ally, "__AuraBuff", 2);
            }
        }
    }



    public List<Position> GetAvailableMoves(Turn turn, Piece piece)
    {
        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        var allPieces = GetAllPiecesFromTurn(turn).Where(p => p.IsAlive).ToList();
        return _movementRulesService.GetAvailableMoves(piece, allPieces);
    }

    /// <summary>
    /// Получает доступные ходы для фигуры с использованием GameSession
    /// </summary>
    public List<Position> GetAvailableMoves(GameSession gameSession, Turn turn, Piece piece)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        var allPieces = gameSession.GetAllPieces();
        return _movementRulesService.GetAvailableMoves(piece, allPieces);
    }

    public List<Position> GetAvailableAttacks(Turn turn, Piece piece)
    {
        if (turn == null)
            throw new ArgumentNullException(nameof(turn));

        if (piece == null)
            throw new ArgumentNullException(nameof(piece));

        var allPieces = GetAllPiecesFromTurn(turn);
        return _attackRulesService.GetAvailableAttacks(piece, allPieces);
    }

    /// <summary>
    /// Получает все фигуры из контекста хода
    /// </summary>
    private List<Piece> GetAllPiecesFromTurn(Turn turn)
    {
        var allPieces = new List<Piece>();
        
        if (turn.ActiveParticipant?.Pieces != null)
        {
            allPieces.AddRange(turn.ActiveParticipant.Pieces);
        }
        
        return allPieces;
    }

    public async Task<bool> ExecuteMoveAsync(GameSession gameSession, Turn turn, Piece piece, Position targetPosition, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(ExecuteMove(gameSession, turn, piece, targetPosition));
    }
}