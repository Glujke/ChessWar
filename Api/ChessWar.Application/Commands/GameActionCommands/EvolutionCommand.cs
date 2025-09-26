using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Application.Interfaces.GameManagement;

namespace ChessWar.Application.Commands.GameActionCommands;

/// <summary>
/// Команда для эволюции фигуры
/// </summary>
public class EvolutionCommand : ICommand
{
    private readonly GameSession _gameSession;
    private readonly Piece _piece;
    private readonly PieceType _targetType;
    private readonly IEvolutionService _evolutionService;
    private readonly IGameNotificationService? _notificationService;

    public EvolutionCommand(
        GameSession gameSession,
        Piece piece,
        PieceType targetType,
        IEvolutionService evolutionService,
        IGameNotificationService? notificationService = null)
    {
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _targetType = targetType;
        _evolutionService = evolutionService ?? throw new ArgumentNullException(nameof(evolutionService));
        _notificationService = notificationService;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var possibleEvolutions = _evolutionService.GetPossibleEvolutions(_piece.Type);
        if (!possibleEvolutions.Contains(_targetType))
        {
            throw new InvalidOperationException($"Piece of type {_piece.Type} cannot evolve to {_targetType}. Allowed evolutions: {string.Join(", ", possibleEvolutions)}");
        }

        if (!_evolutionService.MeetsEvolutionRequirements(_piece, _targetType))
        {
            if (_piece.Type != PieceType.Pawn)
            {
                return await Task.FromResult(false);
            }
        }

        var evolved = _evolutionService.EvolvePiece(_piece, _targetType);

        evolved.Id = _piece.Id;
        evolved.Owner = _piece.Owner;
        var list = _piece.Owner!.Pieces;
        var idx = list.FindIndex(p => p.Id == _piece.Id);
        list[idx] = evolved;
        var pos = _piece.Position;
        _gameSession.Board.RemovePiece(_piece);
        evolved.Position = pos;
        _gameSession.Board.PlacePiece(evolved);
        
        if (_notificationService != null)
        {
            await _notificationService.NotifyPieceEvolvedAsync(
                _gameSession.Id,
                evolved.Id.ToString(),
                evolved.Type.ToString(),
                evolved.Position.X,
                evolved.Position.Y,
                cancellationToken);
        }

        return await Task.FromResult(true);
    }
}
