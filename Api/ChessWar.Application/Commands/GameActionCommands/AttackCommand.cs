using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Commands.GameActionCommands;

/// <summary>
/// Команда для выполнения атаки
/// </summary>
public class AttackCommand : ICommand
{
    private readonly GameSession _gameSession;
    private readonly Turn _turn;
    private readonly Piece _attacker;
    private readonly Position _targetPosition;
    private readonly ITurnService _turnService;
    private readonly ITurnActionRecorder _actionRecorder;

    public AttackCommand(
        GameSession gameSession,
        Turn turn,
        Piece attacker,
        Position targetPosition,
        ITurnService turnService,
        ITurnActionRecorder actionRecorder)
    {
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _turn = turn ?? throw new ArgumentNullException(nameof(turn));
        _attacker = attacker ?? throw new ArgumentNullException(nameof(attacker));
        _targetPosition = targetPosition ?? throw new ArgumentNullException(nameof(targetPosition));
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _actionRecorder = actionRecorder ?? throw new ArgumentNullException(nameof(actionRecorder));
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await Task.FromResult(_turnService.ExecuteAttack(_gameSession, _turn, _attacker, _targetPosition));
        
        return result;
    }
}
