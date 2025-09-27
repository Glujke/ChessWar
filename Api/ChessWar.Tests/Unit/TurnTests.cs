using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit;

public class TurnTests
{
    [Fact]
    public void Constructor_ShouldCreateTurnWithCorrectProperties()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turnNumber = 1;

        var turn = new Turn(turnNumber, player);

        Assert.Equal(turnNumber, turn.Number);
        Assert.Equal(player, turn.ActiveParticipant);
        Assert.Null(turn.SelectedPiece);
        Assert.Empty(turn.Actions);
        Assert.True(turn.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Constructor_WithInvalidTurnNumber_ShouldThrowArgumentException()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var invalidTurnNumber = 0;

        Assert.Throws<ArgumentException>(() => new Turn(invalidTurnNumber, player));
    }

    [Fact]
    public void Constructor_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        var turnNumber = 1;

        Assert.Throws<ArgumentNullException>(() => new Turn(turnNumber, null!));
    }

    [Fact]
    public void SelectPiece_ShouldSetSelectedPiece()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turn = new Turn(1, player);
        var piece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Elves, new Position(1, 1), player);

        turn.SelectPiece(piece);

        Assert.Equal(piece, turn.SelectedPiece);
        Assert.True(turn.HasSelectedPiece());
    }

    [Fact]
    public void SelectPiece_WithWrongOwner_ShouldThrowInvalidOperationException()
    {
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        var turn = new Turn(1, player1);
        var piece = TestHelpers.CreatePiece(PieceType.Pawn, Team.Orcs, new Position(1, 1), player2);

        Assert.Throws<InvalidOperationException>(() => turn.SelectPiece(piece));
    }

    [Fact]
    public void SelectPiece_WithNullPiece_ShouldThrowArgumentNullException()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turn = new Turn(1, player);

        Assert.Throws<ArgumentNullException>(() => turn.SelectPiece(null!));
    }

    [Fact]
    public void AddAction_ShouldAddActionToTurn()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turn = new Turn(1, player);
        var action = new TurnAction("Move", "piece1", new Position(1, 2));

        turn.AddAction(action);

        Assert.Single(turn.Actions);
        Assert.Equal(action, turn.Actions[0]);
    }

    [Fact]
    public void AddAction_WithNullAction_ShouldThrowArgumentNullException()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turn = new Turn(1, player);

        Assert.Throws<ArgumentNullException>(() => turn.AddAction(null!));
    }

    [Fact]
    public void GetActionsByType_ShouldReturnCorrectActions()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turn = new Turn(1, player);
        var moveAction = new TurnAction("Move", "piece1", new Position(1, 2));
        var attackAction = new TurnAction("Attack", "piece1", new Position(3, 4));
        var anotherMoveAction = new TurnAction("Move", "piece2", new Position(5, 6));

        turn.AddAction(moveAction);
        turn.AddAction(attackAction);
        turn.AddAction(anotherMoveAction);

        var moveActions = turn.GetActionsByType("Move");

        Assert.Equal(2, moveActions.Count);
        Assert.Contains(moveAction, moveActions);
        Assert.Contains(anotherMoveAction, moveActions);
    }

    [Fact]
    public void ClearActions_ShouldRemoveAllActions()
    {
        var player = new Player("TestPlayer", new List<Piece>());
        var turn = new Turn(1, player);
        var action1 = new TurnAction("Move", "piece1", new Position(1, 2));
        var action2 = new TurnAction("Attack", "piece1", new Position(3, 4));

        turn.AddAction(action1);
        turn.AddAction(action2);

        turn.ClearActions();

        Assert.Empty(turn.Actions);
    }
}
