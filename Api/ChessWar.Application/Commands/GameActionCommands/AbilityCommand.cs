using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.GameLogic; using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Application.Commands.GameActionCommands;

/// <summary>
/// Команда для использования способности
/// </summary>
public class AbilityCommand : ICommand
{
    private readonly GameSession _gameSession;
    private readonly Piece _piece;
    private readonly Position _targetPosition;
    private readonly string _abilityName;
    private readonly IAbilityService _abilityService;
    private readonly ITurnActionRecorder _actionRecorder;

    public AbilityCommand(
        GameSession gameSession,
        Piece piece,
        Position targetPosition,
        string abilityName,
        IAbilityService abilityService,
        ITurnActionRecorder actionRecorder)
    {
        _gameSession = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
        _piece = piece ?? throw new ArgumentNullException(nameof(piece));
        _targetPosition = targetPosition ?? throw new ArgumentNullException(nameof(targetPosition));
        _abilityName = abilityName ?? throw new ArgumentNullException(nameof(abilityName));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _actionRecorder = actionRecorder ?? throw new ArgumentNullException(nameof(actionRecorder));
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var allPieces = _gameSession.GetAllPieces();
        var success = _abilityService.UseAbility(_piece, _abilityName, _targetPosition, allPieces);
        
        if (success)
        {
            _actionRecorder.RecordAction("Ability", _piece.Id.ToString(), _targetPosition, _abilityName);
        }
        
        return await Task.FromResult(success);
    }
}
