using ChessWar.Application.DTOs;
using ChessWar.Application.Commands.GameActionCommands;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Application.Interfaces.GameManagement;

namespace ChessWar.Application.Commands;

/// <summary>
/// Фабрика для создания команд
/// </summary>
public class CommandFactory : ICommandFactory
{
    private readonly ITurnService _turnService;
    private readonly IAbilityService _abilityService;
    private readonly IEvolutionService _evolutionService;
    private readonly IGameNotificationService _notificationService;

    public CommandFactory(
        ITurnService turnService,
        IAbilityService abilityService,
        IEvolutionService evolutionService,
        IGameNotificationService notificationService)
    {
        _turnService = turnService ?? throw new ArgumentNullException(nameof(turnService));
        _abilityService = abilityService ?? throw new ArgumentNullException(nameof(abilityService));
        _evolutionService = evolutionService ?? throw new ArgumentNullException(nameof(evolutionService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public ICommand? CreateCommand(string actionType, GameSession gameSession, Turn turn, Piece piece, PositionDto? targetPosition, string? description)
    {
        return actionType switch
        {
            "Move" => CreateMoveCommand(gameSession, turn, piece, targetPosition),
            "Attack" => CreateAttackCommand(gameSession, turn, piece, targetPosition),
            "Ability" => CreateAbilityCommand(gameSession, piece, targetPosition, description),
            "Evolve" => CreateEvolutionCommand(gameSession, piece, description),
            _ => null
        };
    }

    private ICommand? CreateMoveCommand(GameSession gameSession, Turn turn, Piece piece, PositionDto? targetPosition)
    {
        if (targetPosition == null) return null;
        var target = new Position(targetPosition.X, targetPosition.Y);
        var actionRecorder = new TurnActionRecorder(turn);
        return new MoveCommand(gameSession, turn, piece, target, _turnService, actionRecorder);
    }

    private ICommand? CreateAttackCommand(GameSession gameSession, Turn turn, Piece piece, PositionDto? targetPosition)
    {
        if (targetPosition == null) return null;
        var target = new Position(targetPosition.X, targetPosition.Y);
        var actionRecorder = new TurnActionRecorder(turn);
        return new AttackCommand(gameSession, turn, piece, target, _turnService, actionRecorder);
    }

    private ICommand? CreateAbilityCommand(GameSession gameSession, Piece piece, PositionDto? targetPosition, string? description)
    {
        if (targetPosition == null || string.IsNullOrWhiteSpace(description)) return null;
        var target = new Position(targetPosition.X, targetPosition.Y);
        var currentTurn = gameSession.GetCurrentTurn();
        var actionRecorder = new TurnActionRecorder(currentTurn);
        return new AbilityCommand(gameSession, piece, target, description, _abilityService, actionRecorder);
    }

    private ICommand? CreateEvolutionCommand(GameSession gameSession, Piece piece, string? description)
    {
        if (string.IsNullOrWhiteSpace(description)) return null;
        if (!Enum.TryParse<PieceType>(description, out var targetType)) return null;
        return new EvolutionCommand(gameSession, piece, targetType, _evolutionService, _notificationService);
    }
}
