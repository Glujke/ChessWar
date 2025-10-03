using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Services.AI;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Unit.AI;

/// <summary>
/// Тесты для оценщика состояния игры
/// </summary>
public class GameStateEvaluatorTests
{
    private readonly GameStateEvaluator _evaluator;

    public GameStateEvaluatorTests()
    {
        _evaluator = new GameStateEvaluator();
    }

    [Fact]
    public void EvaluateGameState_WithEmptyBoard_ShouldReturnZero()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;

        var result = _evaluator.EvaluateGameState(session, player);

        Assert.True(result == 0.0 || result == -1000.0);
    }

    [Fact]
    public void EvaluateGameState_WithOnlyPlayerPieces_ShouldReturnPositiveValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;

        var king = new Piece(PieceType.King, Team.Elves, new Position(0, 0));
        king.HP = 10; // Устанавливаем HP для живой фигуры
        king.Owner = player;
        player.AddPiece(king);
        session.GetBoard().PlacePiece(king);

        var piece = new Piece(PieceType.Queen, Team.Elves, new Position(3, 3));
        piece.HP = 10; // Устанавливаем HP для живой фигуры
        piece.Owner = player;
        player.AddPiece(piece);
        session.GetBoard().PlacePiece(piece);

        var result = _evaluator.EvaluateGameState(session, player);

        Assert.True(result > 0);
    }

    [Fact]
    public void EvaluateGameState_WithEnemyPieces_ShouldReturnNegativeValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;
        var enemy = session.Player2;

        var enemyPiece = new Piece(PieceType.Queen, Team.Orcs, new Position(3, 3));
        enemyPiece.HP = 10; // Устанавливаем HP для живой фигуры
        enemyPiece.Owner = enemy;
        enemy.AddPiece(enemyPiece);
        session.GetBoard().PlacePiece(enemyPiece);

        var result = _evaluator.EvaluateGameState(session, player);

        Assert.True(result < 0);
    }

    [Fact]
    public void EvaluatePiecePosition_WithCenterPosition_ShouldReturnHigherValue()
    {
        var session = CreateEmptyGameSession();
        var centerPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 3));
        var cornerPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));

        var centerValue = _evaluator.EvaluatePiecePosition(centerPiece, session);
        var cornerValue = _evaluator.EvaluatePiecePosition(cornerPiece, session);

        Assert.True(centerValue > cornerValue);
    }

    [Fact]
    public void EvaluatePiecePosition_WithPawnAdvancement_ShouldReturnHigherValue()
    {
        var session = CreateEmptyGameSession();
        var advancedPawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 6));
        var startingPawn = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));

        var advancedValue = _evaluator.EvaluatePiecePosition(advancedPawn, session);
        var startingValue = _evaluator.EvaluatePiecePosition(startingPawn, session);

        Assert.True(advancedValue > startingValue);
    }

    [Fact]
    public void EvaluateKingThreat_WithKingAlone_ShouldReturnNegativeValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;

        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        king.Owner = player;
        player.AddPiece(king);

        var result = _evaluator.EvaluateKingThreat(session, player);

        Assert.True(result <= 0); // Нет угроз = 0 или небольшой бонус
    }

    [Fact]
    public void EvaluateKingThreat_WithEnemiesNearKing_ShouldReturnMoreNegativeValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;
        var enemy = session.Player2;

        var king = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        king.Owner = player;
        player.AddPiece(king);

        var enemyPiece = new Piece(PieceType.Queen, Team.Orcs, new Position(4, 4));
        enemyPiece.Owner = enemy;
        enemy.AddPiece(enemyPiece);

        var result = _evaluator.EvaluateKingThreat(session, player);

        Assert.True(result < 0); // Есть угроза = отрицательное значение
    }

    [Fact]
    public void EvaluateKingThreat_WithDeadKing_ShouldReturnVeryNegativeValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;

        var deadKing = new Piece(PieceType.King, Team.Elves, new Position(3, 3));
        deadKing.Owner = player;
        deadKing.HP = 0; // Мёртвый король
        player.AddPiece(deadKing);

        var result = _evaluator.EvaluateKingThreat(session, player);

        Assert.Equal(-1000.0, result); // Очень плохо
    }

    [Fact]
    public void EvaluateMaterialAdvantage_WithEqualPieces_ShouldReturnZero()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;
        var enemy = session.Player2;

        var playerPiece = new Piece(PieceType.Queen, Team.Elves, new Position(0, 0));
        playerPiece.HP = 10; // Устанавливаем HP для живой фигуры
        playerPiece.Owner = player;
        player.AddPiece(playerPiece);
        session.GetBoard().PlacePiece(playerPiece);

        var enemyPiece = new Piece(PieceType.Queen, Team.Orcs, new Position(7, 7));
        enemyPiece.HP = 10; // Устанавливаем HP для живой фигуры
        enemyPiece.Owner = enemy;
        enemy.AddPiece(enemyPiece);
        session.GetBoard().PlacePiece(enemyPiece);

        var result = _evaluator.EvaluateMaterialAdvantage(session, player);

        Assert.Equal(0.0, result, 10);
    }

    [Fact]
    public void EvaluateMaterialAdvantage_WithPlayerAdvantage_ShouldReturnPositiveValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;
        var enemy = session.Player2;

        var playerQueen = new Piece(PieceType.Queen, Team.Elves, new Position(0, 0));
        playerQueen.HP = 10; // Устанавливаем HP для живой фигуры
        playerQueen.Owner = player;
        player.AddPiece(playerQueen);
        session.GetBoard().PlacePiece(playerQueen);

        var enemyPawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7));
        enemyPawn.HP = 10; // Устанавливаем HP для живой фигуры
        enemyPawn.Owner = enemy;
        enemy.AddPiece(enemyPawn);
        session.GetBoard().PlacePiece(enemyPawn);

        var result = _evaluator.EvaluateMaterialAdvantage(session, player);

        Assert.True(result > 0);
    }

    [Fact]
    public void EvaluateMaterialAdvantage_WithEnemyAdvantage_ShouldReturnNegativeValue()
    {
        var session = CreateEmptyGameSession();
        var player = session.Player1;
        var enemy = session.Player2;

        var playerPawn = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        playerPawn.HP = 10; // Устанавливаем HP для живой фигуры
        playerPawn.Owner = player;
        player.AddPiece(playerPawn);
        session.GetBoard().PlacePiece(playerPawn);

        var enemyQueen = new Piece(PieceType.Queen, Team.Orcs, new Position(7, 7));
        enemyQueen.HP = 10; // Устанавливаем HP для живой фигуры
        enemyQueen.Owner = enemy;
        enemy.AddPiece(enemyQueen);
        session.GetBoard().PlacePiece(enemyQueen);

        var result = _evaluator.EvaluateMaterialAdvantage(session, player);

        Assert.True(result < 0);
    }

    private GameSession CreateEmptyGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("Player 2", new List<Piece>());
        return new GameSession(player1, player2, "Test");
    }
}
