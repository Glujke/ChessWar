using ChessWar.Application.DTOs;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Commands;
using ChessWar.Domain.Entities;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.TurnManagement; using ChessWar.Domain.Interfaces.DataAccess;

namespace ChessWar.Application.Services.Board;

/// <summary>
/// Сервис выполнения действий в ходе
/// </summary>
public class ActionExecutionService : IActionExecutionService
{
    private readonly ITurnService _turnService;
    private readonly IPlayerManagementService _playerManagementService;
    private readonly ICommandFactory _commandFactory;
    private readonly IGameSessionRepository _sessionRepository;

    public ActionExecutionService(
        ITurnService turnService,
        IPlayerManagementService playerManagementService,
        ICommandFactory commandFactory,
        IGameSessionRepository sessionRepository)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _playerManagementService = playerManagementService ?? throw new ArgumentNullException(nameof(playerManagementService));
        _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
    }

    public async Task<bool> ExecuteActionAsync(GameSession gameSession, ExecuteActionDto dto, CancellationToken cancellationToken = default)
    {
        if (gameSession == null)
            throw new ArgumentNullException(nameof(gameSession));

        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var currentTurn = gameSession.GetCurrentTurn();
        var piece = _playerManagementService.FindPieceById(gameSession, dto.PieceId);

        
        if (piece != null)
        {
            try
            {
                
            }
            catch (Exception)
            {
                
            }
        }

        try
        {
            
            if (piece == null || !piece.IsAlive)
            {
                return false;
            }
            
        }
        catch (Exception)
        {
            return false;
        }

        if (piece.AbilityCooldowns.GetValueOrDefault("__RoyalCommandGranted", 0) > 0)
        {
            piece.AbilityCooldowns["__RoyalCommandGranted"] = 0;
        }
        currentTurn.SelectPiece(piece);

        var command = _commandFactory.CreateCommand(dto.Type, gameSession, currentTurn, piece, dto.TargetPosition, dto.Description);
        
        var success = command != null && await command.ExecuteAsync(cancellationToken);

        if (success)
        {
            await _sessionRepository.SaveAsync(gameSession, cancellationToken);
        }

        return success;
    }

    public async Task<bool> ExecuteMoveAsync(GameSession gameSession, Turn turn, Piece piece, Position targetPosition, CancellationToken cancellationToken = default)
    {
        return await _turnService.ExecuteMoveAsync(gameSession, turn, piece, targetPosition, cancellationToken);
    }
}
