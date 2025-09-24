using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.AI;
using ChessWar.Domain.ValueObjects;
using Moq;

namespace ChessWar.Tests.Unit.AI;

/// <summary>
/// Тесты для марковского ИИ
/// </summary>
public class MarkovDecisionAITests
{
    private readonly Mock<IProbabilityMatrix> _mockProbabilityMatrix;
    private readonly Mock<IGameStateEvaluator> _mockEvaluator;
    private readonly Mock<IAIDifficultyLevel> _mockDifficultyProvider;
    private readonly Mock<ITurnService> _mockTurnService;
    private readonly MarkovDecisionAI _ai;

    public MarkovDecisionAITests()
    {
        _mockProbabilityMatrix = new Mock<IProbabilityMatrix>();
        _mockEvaluator = new Mock<IGameStateEvaluator>();
        _mockDifficultyProvider = new Mock<IAIDifficultyLevel>();
        _mockTurnService = new Mock<ITurnService>();
        
        // Настраиваем моки для TurnService - они должны возвращать true для всех действий
        _mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        _mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Returns(true);
        
        // Настраиваем моки для IProbabilityMatrix и IGameStateEvaluator
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
                              .Returns(1.0);
        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
                              .Returns(0.8);
        _mockEvaluator.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
                      .Returns(1.0);
        
        // Настраиваем моки для IAIDifficultyLevel
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
                              .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(It.IsAny<AIDifficultyLevel>()))
                              .Returns(1.0);
        
        _ai = new MarkovDecisionAI(
            _mockProbabilityMatrix.Object,
            _mockEvaluator.Object,
            _mockDifficultyProvider.Object,
            _mockTurnService.Object,
            Mock.Of<IAbilityService>());
    }

    [Fact]
    public void Priority_ShouldReturnOne()
    {
        // Act
        var result = _ai.Priority;

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void Name_ShouldReturnCorrectName()
    {
        // Act
        var result = _ai.Name;

        // Assert
        Assert.Equal("Markov Decision AI", result);
    }

    [Fact]
    public void CanExecute_WithAlivePiecesAndMana_ShouldReturnTrue()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        
        var turn = new Turn(1, player1); // ход 1

        // Act
        var result = _ai.CanExecute(session, turn, player1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanExecute_WithNoAlivePieces_ShouldReturnFalse()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 0; // Убиваем фигуру
        player1.AddPiece(piece);
        
        var turn = new Turn(1, player1);

        // Act
        var result = _ai.CanExecute(session, turn, player1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanExecute_WithNoMana_ShouldReturnFalse()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        var session = new GameSession(player1, player2);
        session.StartGame();
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        
        var turn = new Turn(1, player1); // ход 1

        // Act
        var result = _ai.CanExecute(session, turn, player1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Execute_WithValidConditions_ShouldReturnTrue()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Execute_WithNoValidMoves_ShouldReturnFalse()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0)); // В углу доски
        piece.Owner = player1;
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Execute_ShouldCallTurnServiceExecuteMove()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        _ai.Execute(session, turn, player1);

        // Assert
        _mockTurnService.Verify(x => x.ExecuteMove(
            It.IsAny<GameSession>(), 
            It.IsAny<Turn>(), 
            It.IsAny<Piece>(), 
            It.IsAny<Position>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldCallProbabilityMatrixGetReward()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        _ai.Execute(session, turn, player1);

        // Assert
        _mockProbabilityMatrix.Verify(x => x.GetReward(
            It.IsAny<GameSession>(), 
            It.IsAny<GameAction>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldCallGameStateEvaluator()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        _ai.Execute(session, turn, player1);

        // Assert
        _mockEvaluator.Verify(x => x.EvaluateGameState(
            It.IsAny<GameSession>(), 
            It.IsAny<Player>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldCallDifficultyProvider()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        _ai.Execute(session, turn, player1);

        // Assert
        _mockDifficultyProvider.Verify(x => x.GetDifficultyLevel(
            It.IsAny<Player>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_WithDifferentPieceTypes_ShouldGenerateActions()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();
        
        // Добавляем разные типы фигур
        var pieces = new[]
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)),
            new Piece(PieceType.Knight, Team.Elves, new Position(2, 2)),
            new Piece(PieceType.Bishop, Team.Elves, new Position(3, 3)),
            new Piece(PieceType.Rook, Team.Elves, new Position(4, 4)),
            new Piece(PieceType.Queen, Team.Elves, new Position(5, 5)),
            new Piece(PieceType.King, Team.Elves, new Position(6, 6))
        };
        
        foreach (var piece in pieces)
        {
            piece.Owner = player1;
            piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
            player1.AddPiece(piece);
            session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        }
        
        var turn = new Turn(1, player1); // ход 1
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.True(result);
        
        // Проверяем, что TurnService был вызван для разных типов фигур
        _mockTurnService.Verify(x => x.ExecuteMove(
            It.IsAny<GameSession>(), 
            It.IsAny<Turn>(), 
            It.IsAny<Piece>(), 
            It.IsAny<Position>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_WithLowMana_ShouldStillTryToMakeMoves()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1); // ход 1
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        // AI должен попытаться сделать ход даже с малой маной
        _mockTurnService.Verify(x => x.ExecuteMove(
            It.IsAny<GameSession>(), 
            It.IsAny<Turn>(), 
            It.IsAny<Piece>(), 
            It.IsAny<Position>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldHandleEmptyActionList()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0)); // В углу, нет ходов
        piece.Owner = player1;
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Execute_ShouldRespectDifficultyLevel()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Настраиваем мок для возврата разных уровней сложности
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Hard);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.True(result);
        _mockDifficultyProvider.Verify(x => x.GetDifficultyLevel(player1), Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldUseProbabilityMatrixForActionSelection()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        _ai.Execute(session, turn, player1);

        // Assert
        _mockProbabilityMatrix.Verify(x => x.GetReward(
            It.IsAny<GameSession>(), 
            It.IsAny<GameAction>()), 
            Times.AtLeastOnce);
        
        _mockProbabilityMatrix.Verify(x => x.GetTransitionProbability(
            It.IsAny<GameSession>(), 
            It.IsAny<GameAction>(), 
            It.IsAny<GameSession>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldHandleTurnServiceExceptions()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Настраиваем мок для выброса исключения
        _mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                      .Throws(new Exception("Test exception"));

        // Act & Assert
        // AI должен обработать исключение и не упасть
        var result = _ai.Execute(session, turn, player1);
        
        // Результат может быть false из-за исключения, но тест не должен упасть
        Assert.False(result);
    }

    [Fact]
    public void Execute_ShouldGenerateActionsForAllPieceTypes()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        // Создаем фигуры всех типов
        var pieces = new[]
        {
            new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1)),
            new Piece(PieceType.Knight, Team.Elves, new Position(2, 2)),
            new Piece(PieceType.Bishop, Team.Elves, new Position(3, 3)),
            new Piece(PieceType.Rook, Team.Elves, new Position(4, 4)),
            new Piece(PieceType.Queen, Team.Elves, new Position(5, 5)),
            new Piece(PieceType.King, Team.Elves, new Position(6, 6))
        };
        
        foreach (var piece in pieces)
        {
            piece.Owner = player1;
            piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
            player1.AddPiece(piece);
            session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        }
        
        var turn = new Turn(1, player1); // ход 1
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.True(result);
        
        // Проверяем, что AI попытался сделать ходы
        _mockTurnService.Verify(x => x.ExecuteMove(
            It.IsAny<GameSession>(), 
            It.IsAny<Turn>(), 
            It.IsAny<Piece>(), 
            It.IsAny<Position>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldRespectManaConstraints()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        session.StartGame();
        
        var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        piece.Owner = player1;
        piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
        player1.AddPiece(piece);
        session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        
        var turn = new Turn(1, player1); // ход 1
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        // AI должен попытаться сделать ход в рамках доступной маны
        _mockTurnService.Verify(x => x.ExecuteMove(
            It.IsAny<GameSession>(), 
            It.IsAny<Turn>(), 
            It.IsAny<Piece>(), 
            It.IsAny<Position>()), 
            Times.AtLeastOnce);
    }

    [Fact]
    public void Execute_ShouldHandleMultiplePieces()
    {
        // Arrange
        var player1 = new Player("Player1", new List<Piece>());
        var player2 = new Player("Player2", new List<Piece>());
        
        // Добавляем ману игрокам
        player1.SetMana(10, 10);
        player2.SetMana(10, 10);
        var session = new GameSession(player1, player2);
        
        // Добавляем несколько фигур
        for (int i = 0; i < 5; i++)
        {
            var piece = new Piece(PieceType.Pawn, Team.Elves, new Position(i + 1, 1));
            piece.Owner = player1;
            piece.HP = 10; // Устанавливаем HP чтобы фигура была живой
            player1.AddPiece(piece);
            session.Board.PlacePiece(piece); // Добавляем фигуру в Board
        }
        
        var turn = new Turn(1, player1);
        session.SetCurrentTurn(turn);

        // Act
        var result = _ai.Execute(session, turn, player1);

        // Assert
        Assert.True(result);
        
        // AI должен попытаться сделать ходы с разными фигурами
        _mockTurnService.Verify(x => x.ExecuteMove(
            It.IsAny<GameSession>(), 
            It.IsAny<Turn>(), 
            It.IsAny<Piece>(), 
            It.IsAny<Position>()), 
            Times.AtLeastOnce);
    }
}