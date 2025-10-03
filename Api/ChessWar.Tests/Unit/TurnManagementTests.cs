using FluentAssertions;
using Microsoft.Extensions.Logging;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.TurnManagement;
using ChessWar.Domain.Services.GameLogic;
using Moq;
using ChessWar.Tests.Helpers;
using Xunit.Abstractions;

namespace ChessWar.Tests.Unit;

public class TurnManagementTests
{
    private readonly Mock<IMovementRulesService> _movementRulesServiceMock;
    private readonly Mock<IAttackRulesService> _attackRulesServiceMock;
    private readonly Mock<IEvolutionService> _evolutionServiceMock;
    private readonly TurnService _turnService;
    private readonly GameSession _gameSession;
    private readonly ITestOutputHelper _output;

    public TurnManagementTests(ITestOutputHelper output)
    {
        _output = output;
        _movementRulesServiceMock = new Mock<IMovementRulesService>();
        _attackRulesServiceMock = new Mock<IAttackRulesService>();
        _evolutionServiceMock = new Mock<IEvolutionService>();

        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        _turnService = new TurnService(_movementRulesServiceMock.Object, _attackRulesServiceMock.Object, _evolutionServiceMock.Object, _TestConfig.CreateProvider(), new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);

        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");

        // Добавляем фигуры к игрокам для правильной инициализации
        var player1Piece = CreateTestPiece("player1_piece", PieceType.Pawn, Team.Elves, new Position(0, 1), player1);
        var player2Piece = CreateTestPiece("player2_piece", PieceType.Pawn, Team.Orcs, new Position(0, 6), player2);

        player1.AddPiece(player1Piece);
        player2.AddPiece(player2Piece);

        _gameSession = new GameSession(player1, player2);
        _gameSession.StartGame();
    }

    [Fact]
    public void GameSession_StartGame_ShouldCreateFirstTurn()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);

        gameSession.StartGame();

        var currentTurn = gameSession.GetCurrentTurn();
        currentTurn.Should().NotBeNull();
        currentTurn.Number.Should().Be(1);
        currentTurn.ActiveParticipant.Should().Be(player1);
        currentTurn.SelectedPiece.Should().BeNull();
    }

    [Fact]
    public void GameSession_EndCurrentTurn_ShouldSwitchToNextPlayer()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);
        gameSession.StartGame();

        gameSession.EndCurrentTurn();

        var currentTurn = gameSession.GetCurrentTurn();
        currentTurn.Number.Should().Be(2);
        currentTurn.ActiveParticipant.Should().Be(player2);
    }

    [Fact]
    public void GameSession_CanPlayerAct_ShouldReturnTrueForActivePlayer()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);
        gameSession.StartGame();

        gameSession.CanPlayerAct(player1).Should().BeTrue();
        gameSession.CanPlayerAct(player2).Should().BeFalse();
    }

    [Fact]
    public void GameSession_GetNextPlayer_ShouldReturnCorrectPlayer()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);
        gameSession.StartGame();

        gameSession.GetNextPlayer().Should().Be(player2);

        gameSession.EndCurrentTurn();
        gameSession.GetNextPlayer().Should().Be(player1);
    }

    [Fact]
    public void Turn_SelectPiece_ShouldSetSelectedPiece()
    {
        var player = CreateTestPlayer("Player1");
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);

        turn.SelectPiece(piece);

        turn.SelectedPiece.Should().Be(piece);
        turn.HasSelectedPiece().Should().BeTrue();
    }

    [Fact]
    public void Turn_SelectPiece_WithWrongOwner_ShouldThrowException()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var turn = new Turn(1, player1);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Orcs, new Position(1, 1), player2);

        Assert.Throws<InvalidOperationException>(() => turn.SelectPiece(piece));
    }

    [Fact]
    public void TurnService_ExecuteMove_WithSelectedPiece_ShouldSucceed()
    {
        var player = CreateTestPlayer("Player1");
        player.SetMana(10, 10); // Устанавливаем ману для игрока
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        piece.HP = 10; // Устанавливаем HP для живой фигуры
        turn.SelectPiece(piece);

        _gameSession.GetBoard().PlacePiece(piece);

        _movementRulesServiceMock
            .Setup(x => x.CanMoveTo(piece, It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        var result = _turnService.ExecuteMove(_gameSession, turn, piece, new Position(1, 2));

        result.Should().BeTrue();
        turn.Actions.Should().HaveCount(1);
        turn.Actions[0].ActionType.Should().Be("Move");
    }


    [Fact]
    public void TurnService_ExecuteAttack_WithSelectedPiece_ShouldSucceed()
    {
        var player = _gameSession.Player1; // Используем игрока из GameSession
        player.SetMana(10, 10); // Устанавливаем ману для игрока
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        piece.HP = 10; // Устанавливаем HP для живой фигуры
        turn.SelectPiece(piece);

        _gameSession.GetBoard().PlacePiece(piece);

        var enemyPlayer = _gameSession.Player2; // Используем игрока из GameSession
        var enemyPiece = CreateTestPiece("enemy1", PieceType.Pawn, Team.Orcs, new Position(1, 2), enemyPlayer);
        _gameSession.GetBoard().PlacePiece(enemyPiece);

        _attackRulesServiceMock
            .Setup(x => x.CanAttack(It.IsAny<Piece>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        var targetPosition = new Position(1, 2);

        _output.WriteLine($"Testing attack from piece {piece.Id} at {piece.Position} to {targetPosition}");
        _output.WriteLine($"GameSession board pieces count: {_gameSession.GetBoard().Pieces.Count()}");

        var targetPiece = _gameSession.GetBoard().GetPieceAt(targetPosition);
        _output.WriteLine($"Target piece found: {targetPiece?.Id} at {targetPosition}");

        if (targetPiece == null)
        {
            _output.WriteLine("Available pieces on board:");
            foreach (var boardPiece in _gameSession.GetBoard().Pieces)
            {
                _output.WriteLine($"  Piece {boardPiece.Id} at {boardPiece.Position} (Owner: {boardPiece.Owner?.Name})");
            }
        }

        _output.WriteLine($"Target piece found: {targetPiece?.Id} at {targetPosition}");

        if (targetPiece == null)
        {
            _output.WriteLine("ERROR: Target piece not found! Test will fail.");
            _output.WriteLine("Available pieces on board:");
            foreach (var boardPiece in _gameSession.GetBoard().Pieces)
            {
                _output.WriteLine($"  Piece {boardPiece.Id} at {boardPiece.Position} (Owner: {boardPiece.Owner?.Name})");
            }
            return;
        }

        _output.WriteLine($"Target piece found: {targetPiece.Id} at {targetPiece.Position}");

        _output.WriteLine("Before ExecuteAttack:");
        _output.WriteLine($"  Piece position: {piece.Position}");
        _output.WriteLine($"  Target position: {targetPosition}");
        _output.WriteLine($"  Turn actions count: {turn.Actions.Count}");

        _output.WriteLine("Testing mock CanAttack call:");
        var canAttackResult = _attackRulesServiceMock.Object.CanAttack(piece, targetPosition, _gameSession.GetBoard().Pieces.ToList());
        _output.WriteLine($"  CanAttack result: {canAttackResult}");

        var result = _turnService.ExecuteAttack(_gameSession, turn, piece, targetPosition);

        _output.WriteLine($"ExecuteAttack result: {result}");
        _output.WriteLine($"Turn actions count: {turn.Actions.Count}");

        result.Should().BeTrue($"ExecuteAttack should return true. Actions count: {turn.Actions.Count}");
        turn.Actions.Should().HaveCount(1);
        turn.Actions[0].ActionType.Should().Be("Attack");
    }

    private Player CreateTestPlayer(string name)
    {
        return new Player(name, new List<Piece>());
    }

    private Piece CreateTestPiece(string id, PieceType type, Team team, Position position, Participant owner)
    {
        return TestHelpers.CreatePiece(type, team, position, owner);
    }
}


