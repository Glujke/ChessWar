using FluentAssertions;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using GameStatus = ChessWar.Domain.Enums.GameStatus;
using GameResult = ChessWar.Domain.Enums.GameResult;

namespace ChessWar.Tests.Unit;

public class GameSessionTests
{
    [Fact]
    public void CreateGameSession_WithValidPlayers_ShouldCreateSession()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");

        // Act
        var session = new GameSession(player1, player2);

        // Assert
        session.Should().NotBeNull();
        session.Player1.Should().Be(player1);
        session.Player2.Should().Be(player2);
        session.Status.Should().Be(GameStatus.Waiting);
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateGameSession_WithNullPlayer1_ShouldThrowArgumentNullException()
    {
        // Arrange
        Player? player1 = null;
        var player2 = CreateTestPlayer("Player2");

        // Act & Assert
        var action = () => new GameSession(player1!, player2);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("player1");
    }

    [Fact]
    public void CreateGameSession_WithNullPlayer2_ShouldThrowArgumentNullException()
    {
        // Arrange
        var player1 = CreateTestPlayer("Player1");
        Player? player2 = null;

        // Act & Assert
        var action = () => new GameSession(player1, player2!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("player2");
    }

    [Fact]
    public void StartGame_ShouldChangeStatusToActive()
    {
        // Arrange
        var session = CreateTestGameSession();

        // Act
        session.StartGame();

        // Assert
        session.Status.Should().Be(GameStatus.Active);
    }

    [Fact]
    public void StartGame_WhenAlreadyActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var session = CreateTestGameSession();
        session.StartGame();

        // Act & Assert
        var action = () => session.StartGame();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Game is already active");
    }

    [Fact]
    public void CompleteGame_WithVictory_ShouldChangeStatusToFinished()
    {
        // Arrange
        var session = CreateTestGameSession();
        session.StartGame();

        // Act
        session.CompleteGame(GameResult.Player1Victory);

        // Assert
        session.Status.Should().Be(GameStatus.Finished);
        session.Result.Should().Be(GameResult.Player1Victory);
    }

    [Fact]
    public void CompleteGame_WithDefeat_ShouldChangeStatusToFinished()
    {
        // Arrange
        var session = CreateTestGameSession();
        session.StartGame();

        // Act
        session.CompleteGame(GameResult.Player2Victory);

        // Assert
        session.Status.Should().Be(GameStatus.Finished);
        session.Result.Should().Be(GameResult.Player2Victory);
    }

    [Fact]
    public void CompleteGame_WhenNotActive_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var session = CreateTestGameSession();

        // Act & Assert
        var action = () => session.CompleteGame(GameResult.Player1Victory);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Game is not active");
    }

    [Fact]
    public void GetCurrentTurn_ShouldReturnCurrentTurn()
    {
        // Arrange
        var session = CreateTestGameSession();
        session.StartGame();

        // Act
        var currentTurn = session.GetCurrentTurn();

        // Assert
        currentTurn.Should().NotBeNull();
        currentTurn.Number.Should().Be(1);
        currentTurn.ActiveParticipant.Should().Be(session.Player1);
    }

    [Fact]
    public void GetCurrentTurn_WhenGameNotStarted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var session = CreateTestGameSession();

        // Act & Assert
        var action = () => session.GetCurrentTurn();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Game has not started");
    }

    [Fact]
    public void GetBoard_ShouldReturnGameBoard()
    {
        // Arrange
        var session = CreateTestGameSession();

        // Act
        var board = session.GetBoard();

        // Assert
        board.Should().NotBeNull();
        board.Should().BeOfType<GameBoard>();
    }

    [Fact]
    public void GetPlayer1Pieces_ShouldReturnPlayer1Pieces()
    {
        // Arrange
        var session = CreateTestGameSession();

        // Act
        var pieces = session.GetPlayer1Pieces();

        // Assert
        pieces.Should().NotBeNull();
        pieces.Should().HaveCount(9); // 8 пешек + 1 король
        pieces.Should().AllSatisfy(piece => piece.Team.Should().Be(Team.Elves));
    }

    [Fact]
    public void GetPlayer2Pieces_ShouldReturnPlayer2Pieces()
    {
        // Arrange
        var session = CreateTestGameSession();

        // Act
        var pieces = session.GetPlayer2Pieces();

        // Assert
        pieces.Should().NotBeNull();
        pieces.Should().HaveCount(9); // 8 пешек + 1 король
        pieces.Should().AllSatisfy(piece => piece.Team.Should().Be(Team.Elves));
    }

    private static GameSession CreateTestGameSession()
    {
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");
        return new GameSession(player1, player2);
    }

    private static Player CreateTestPlayer(string name)
    {
        var pieces = new List<Piece>();
        
        // Создаём 8 пешек для игрока
        for (int i = 0; i < 8; i++)
        {
            pieces.Add(new Piece(PieceType.Pawn, Team.Elves, new Position(i, 1)));
        }
        
        // Создаём короля
        pieces.Add(new Piece(PieceType.King, Team.Elves, new Position(4, 0)));
        
        return new Player(name, pieces);
    }
}
