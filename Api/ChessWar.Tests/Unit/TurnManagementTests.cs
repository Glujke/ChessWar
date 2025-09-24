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

namespace ChessWar.Tests.Unit;

public class TurnManagementTests
{
    private readonly Mock<IMovementRulesService> _movementRulesServiceMock;
    private readonly Mock<IAttackRulesService> _attackRulesServiceMock;
    private readonly Mock<IEvolutionService> _evolutionServiceMock;
    private readonly TurnService _turnService;
    private readonly GameSession _gameSession;

    public TurnManagementTests()
    {
        _movementRulesServiceMock = new Mock<IMovementRulesService>();
        _attackRulesServiceMock = new Mock<IAttackRulesService>();
        _evolutionServiceMock = new Mock<IEvolutionService>();
        
        var turnServiceLogger = Mock.Of<ILogger<TurnService>>();
        _turnService = new TurnService(_movementRulesServiceMock.Object, _attackRulesServiceMock.Object, _evolutionServiceMock.Object, _TestConfig.CreateProvider(), new MockDomainEventDispatcher(), new PieceDomainService(), turnServiceLogger);
        
        // Создаем тестовую игровую сессию
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        _gameSession = new GameSession(player1, player2);
        _gameSession.StartGame();
    }

    [Fact]
    public void GameSession_StartGame_ShouldCreateFirstTurn()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);

        // Act
        gameSession.StartGame();

        // Assert
        var currentTurn = gameSession.GetCurrentTurn();
        currentTurn.Should().NotBeNull();
        currentTurn.Number.Should().Be(1);
        currentTurn.ActiveParticipant.Should().Be(player1);
        currentTurn.SelectedPiece.Should().BeNull();
    }

    [Fact]
    public void GameSession_EndCurrentTurn_ShouldSwitchToNextPlayer()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);
        gameSession.StartGame();

        // Act
        gameSession.EndCurrentTurn();

        // Assert
        var currentTurn = gameSession.GetCurrentTurn();
        currentTurn.Number.Should().Be(2);
        currentTurn.ActiveParticipant.Should().Be(player2);
    }

    [Fact]
    public void GameSession_CanPlayerAct_ShouldReturnTrueForActivePlayer()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);
        gameSession.StartGame();

        // Act & Assert
        gameSession.CanPlayerAct(player1).Should().BeTrue();
        gameSession.CanPlayerAct(player2).Should().BeFalse();
    }

    [Fact]
    public void GameSession_GetNextPlayer_ShouldReturnCorrectPlayer()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var gameSession = new GameSession(player1, player2);
        gameSession.StartGame();

        // Act & Assert
        gameSession.GetNextPlayer().Should().Be(player2);
        
        gameSession.EndCurrentTurn();
        gameSession.GetNextPlayer().Should().Be(player1);
    }

    [Fact]
    public void Turn_SelectPiece_ShouldSetSelectedPiece()
    {
        // Arrange
        var player = CreateTestPlayer("Player1");
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);

        // Act
        turn.SelectPiece(piece);

        // Assert
        turn.SelectedPiece.Should().Be(piece);
        turn.HasSelectedPiece().Should().BeTrue();
    }

    [Fact]
    public void Turn_SelectPiece_WithWrongOwner_ShouldThrowException()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        var turn = new Turn(1, player1);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Orcs, new Position(1, 1), player2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => turn.SelectPiece(piece));
    }

    [Fact]
    public void TurnService_ExecuteMove_WithSelectedPiece_ShouldSucceed()
    {
        // Arrange
        var player = CreateTestPlayer("Player1");
        player.SetMana(10, 10); // Устанавливаем ману для игрока
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        piece.HP = 10; // Устанавливаем HP для живой фигуры
        turn.SelectPiece(piece);
        
        // Размещаем фигуру на доске
        _gameSession.GetBoard().PlacePiece(piece);
        
        _movementRulesServiceMock
            .Setup(x => x.CanMoveTo(piece, It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        // Act
        var result = _turnService.ExecuteMove(_gameSession, turn, piece, new Position(1, 2));

        // Assert
        result.Should().BeTrue();
        turn.Actions.Should().HaveCount(1);
        turn.Actions[0].ActionType.Should().Be("Move");
    }


    [Fact]
    public void TurnService_ExecuteAttack_WithSelectedPiece_ShouldSucceed()
    {
        // Arrange
        var player = CreateTestPlayer("Player1");
        player.SetMana(10, 10); // Устанавливаем ману для игрока
        var turn = new Turn(1, player);
        var piece = CreateTestPiece("piece1", PieceType.Pawn, Team.Elves, new Position(1, 1), player);
        piece.HP = 10; // Устанавливаем HP для живой фигуры
        turn.SelectPiece(piece);
        
        // Размещаем фигуру на доске
        _gameSession.GetBoard().PlacePiece(piece);
        
        _attackRulesServiceMock
            .Setup(x => x.CanAttack(piece, It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Returns(true);

        // Act
        var result = _turnService.ExecuteAttack(_gameSession, turn, piece, new Position(1, 2));

        // Assert
        result.Should().BeTrue();
        turn.Actions.Should().HaveCount(1);
        turn.Actions[0].ActionType.Should().Be("Attack");
    }



    private GameSession CreateTestGameSession()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        return new GameSession(player1, player2);
    }

    private Player CreateTestPlayer(string name)
    {
        return new Player(name, new List<Piece>());
    }

    private Piece CreateTestPiece(string id, PieceType type, Team team, Position position, Player owner)
    {
        return TestHelpers.CreatePiece(type, team, position, owner);
    }
}


