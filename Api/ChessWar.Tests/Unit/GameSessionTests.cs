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
        var player1 = CreateTestPlayer("Player1");
        var player2 = CreateTestPlayer("Player2");

        var session = new GameSession(player1, player2);

        session.Should().NotBeNull();
        session.Player1.Should().Be(player1);
        session.Player2.Should().Be(player2);
        session.Status.Should().Be(GameStatus.Waiting);
        session.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateGameSession_WithNullPlayer1_ShouldThrowArgumentNullException()
    {
        Player? player1 = null;
        var player2 = CreateTestPlayer("Player2");

        var action = () => new GameSession(player1!, player2);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("player1");
    }

    [Fact]
    public void CreateGameSession_WithNullPlayer2_ShouldThrowArgumentNullException()
    {
        var player1 = CreateTestPlayer("Player1");
        Player? player2 = null;

        var action = () => new GameSession(player1, player2!);
        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("player2");
    }

    [Fact]
    public void StartGame_ShouldChangeStatusToActive()
    {
        var session = CreateTestGameSession();

        session.StartGame();

        session.Status.Should().Be(GameStatus.Active);
    }

    [Fact]
    public void StartGame_WhenAlreadyActive_ShouldThrowInvalidOperationException()
    {
        var session = CreateTestGameSession();
        session.StartGame();

        var action = () => session.StartGame();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Game is already active");
    }

    [Fact]
    public void CompleteGame_WithVictory_ShouldChangeStatusToFinished()
    {
        var session = CreateTestGameSession();
        session.StartGame();

        session.CompleteGame(GameResult.Player1Victory);

        session.Status.Should().Be(GameStatus.Finished);
        session.Result.Should().Be(GameResult.Player1Victory);
    }

    [Fact]
    public void CompleteGame_WithDefeat_ShouldChangeStatusToFinished()
    {
        var session = CreateTestGameSession();
        session.StartGame();

        session.CompleteGame(GameResult.Player2Victory);

        session.Status.Should().Be(GameStatus.Finished);
        session.Result.Should().Be(GameResult.Player2Victory);
    }

    [Fact]
    public void CompleteGame_WhenNotActive_ShouldThrowInvalidOperationException()
    {
        var session = CreateTestGameSession();

        var action = () => session.CompleteGame(GameResult.Player1Victory);
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Game is not active");
    }

    [Fact]
    public void GetCurrentTurn_ShouldReturnCurrentTurn()
    {
        var session = CreateTestGameSession();
        session.StartGame();

        var currentTurn = session.GetCurrentTurn();

        currentTurn.Should().NotBeNull();
        currentTurn.Number.Should().Be(1);
        currentTurn.ActiveParticipant.Should().Be(session.Player1);
    }

    [Fact]
    public void GetCurrentTurn_WhenGameNotStarted_ShouldThrowInvalidOperationException()
    {
        var session = CreateTestGameSession();

        var action = () => session.GetCurrentTurn();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Game has not started");
    }

    [Fact]
    public void GetBoard_ShouldReturnGameBoard()
    {
        var session = CreateTestGameSession();

        var board = session.GetBoard();

        board.Should().NotBeNull();
        board.Should().BeOfType<GameBoard>();
    }

    [Fact]
    public void GetPlayer1Pieces_ShouldReturnPlayer1Pieces()
    {
        var session = CreateTestGameSession();

        var pieces = session.GetPlayer1Pieces();

        pieces.Should().NotBeNull();
        pieces.Should().HaveCount(9); // 8 пешек + 1 король
        pieces.Should().AllSatisfy(piece => piece.Team.Should().Be(Team.Elves));
    }

    [Fact]
    public void GetPlayer2Pieces_ShouldReturnPlayer2Pieces()
    {
        var session = CreateTestGameSession();

        var pieces = session.GetPlayer2Pieces();

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
        
        for (int i = 0; i < 8; i++)
        {
            pieces.Add(new Piece(PieceType.Pawn, Team.Elves, new Position(i, 1)));
        }
        
        pieces.Add(new Piece(PieceType.King, Team.Elves, new Position(4, 0)));
        
        return new Player(name, pieces);
    }
}
