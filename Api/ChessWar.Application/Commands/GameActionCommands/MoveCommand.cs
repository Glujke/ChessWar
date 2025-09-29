using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Commands.GameActionCommands;

/// <summary>
/// Команда для выполнения движения фигуры
/// </summary>
public class MoveCommand : ICommand
{
    private readonly GameSession _gameSession;
    private readonly Turn _turn;
    private readonly Piece _piece;
    private readonly Position _targetPosition;
    private readonly ITurnService _turnService;
    private readonly ITurnActionRecorder _actionRecorder;

    public MoveCommand(
        GameSession gameSession,
        Turn turn,
        Piece piece,
        Position targetPosition,
        ITurnService turnService,
        ITurnActionRecorder actionRecorder)
    {
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _targetPosition = targetPosition ?? throw new ArgumentNullException(nameof(targetPosition));
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _actionRecorder = actionRecorder ?? throw new ArgumentNullException(nameof(actionRecorder));
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_turnService.ExecuteMove(_gameSession, _turn, _piece, _targetPosition));
    }
}
