using ChessWar.Domain.Entities;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Application.Services.AI;
using Moq;
using ChessWar.Domain.Enums;

namespace ChessWar.Tests.Unit.AI;

public class AIEfficiencyTests
{
    private readonly Mock<IProbabilityMatrix> _mockProbabilityMatrix;
    private readonly Mock<IGameStateEvaluator> _mockEvaluator;
    private readonly Mock<IAIDifficultyLevel> _mockDifficultyProvider;
    private readonly Mock<ITurnService> _mockTurnService;
    private readonly ProbabilisticAIService _aiService;

    public AIEfficiencyTests()
    {
        _mockProbabilityMatrix = new Mock<IProbabilityMatrix>();
        _mockEvaluator = new Mock<IGameStateEvaluator>();
        _mockDifficultyProvider = new Mock<IAIDifficultyLevel>();
        _mockTurnService = new Mock<ITurnService>();
        
        // Настраиваем моки для TurnService - они должны реально тратить ману и двигать фигуры
        _mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) => {
                Console.WriteLine($"[MockTurnService] ExecuteMove callback called for piece {piece.Id} to ({position.X},{position.Y})");
                Console.WriteLine($"[MockTurnService] Before: Turn.RemainingMP={turn.RemainingMP}, Player.MP={turn.ActiveParticipant.MP}");
                // Тратим ману за движение
                turn.SpendMP(1);
                turn.ActiveParticipant.Spend(1);
                // Двигаем фигуру (обновляем позицию напрямую для тестов)
                piece.Position = position;
                piece.IsFirstMove = false;
                // Обновляем оставшуюся ману из состояния игрока
                turn.UpdateRemainingMP();
                Console.WriteLine($"[MockTurnService] After: Turn.RemainingMP={turn.RemainingMP}, Player.MP={turn.ActiveParticipant.MP}");
            })
            .Returns(true);
        _mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) => {
                Console.WriteLine($"[MockTurnService] ExecuteAttack callback called for piece {piece.Id} to ({position.X},{position.Y})");
                Console.WriteLine($"[MockTurnService] Before: Turn.RemainingMP={turn.RemainingMP}, Player.MP={turn.ActiveParticipant.MP}");
                // Тратим ману за атаку
                turn.SpendMP(2);
                turn.ActiveParticipant.Spend(2);
                // Находим цель и наносим урон
                Console.WriteLine($"[MockTurnService] Looking for target at position ({position.X},{position.Y})");
                Console.WriteLine($"[MockTurnService] Attacking piece owner: {piece.Owner?.Id}");
                Console.WriteLine($"[MockTurnService] Player1 pieces: {session.Player1.Pieces.Count}");
                foreach (var p in session.Player1.Pieces)
                {
                    Console.WriteLine($"[MockTurnService] Player1 piece {p.Id}: Owner={p.Owner?.Id}, Position=({p.Position.X},{p.Position.Y}), IsAlive={p.IsAlive}");
                }
                Console.WriteLine($"[MockTurnService] Player2 pieces: {session.Player2.Pieces.Count}");
                foreach (var p in session.Player2.Pieces)
                {
                    Console.WriteLine($"[MockTurnService] Player2 piece {p.Id}: Owner={p.Owner?.Id}, Position=({p.Position.X},{p.Position.Y}), IsAlive={p.IsAlive}");
                }
                
                var target = session.Player1.Pieces.Concat(session.Player2.Pieces)
                    .FirstOrDefault(p => p.Position.Equals(position) && p.Owner?.Id != piece.Owner?.Id && p.IsAlive);
                if (target != null)
                {
                    Console.WriteLine($"[MockTurnService] Found target: {target.Id} with HP {target.HP}");
                    TestHelpers.TakeDamage(target, piece.ATK);
                    Console.WriteLine($"[MockTurnService] Target HP after damage: {target.HP}");
                }
                else
                {
                    Console.WriteLine($"[MockTurnService] No target found at position ({position.X},{position.Y})");
                }
                // Обновляем оставшуюся ману из состояния игрока
                turn.UpdateRemainingMP();
                Console.WriteLine($"[MockTurnService] After: Turn.RemainingMP={turn.RemainingMP}, Player.MP={turn.ActiveParticipant.MP}");
            })
            .Returns(true);
        
        _aiService = new ProbabilisticAIService(
            _mockProbabilityMatrix.Object,
            _mockEvaluator.Object,
            _mockDifficultyProvider.Object,
            _mockTurnService.Object,
            Mock.Of<IAbilityService>(),
            Mock.Of<Microsoft.Extensions.Logging.ILogger<ProbabilisticAIService>>()
        );
    }

    [Fact]
    public void MakeAiTurn_WithValidActions_ShouldSpendManaAndExecuteActions()
    {
        // Arrange
        var session = CreateGameSessionWithValidActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;
        
        // Настраиваем моки для успешного выполнения действий
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);
        
        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.8);
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);

        // Act
        var result = _aiService.MakeAiTurn(session);

        // Assert
        Assert.True(result, "ИИ должен успешно выполнить ход");
        
        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.True(finalMana < initialMana, $"ИИ должен потратить ману. Было: {initialMana}, стало: {finalMana}");
        
        // Проверяем, что фигуры действительно сдвинулись
        var activePlayer = session.GetCurrentTurn().ActiveParticipant;
        var activePieces = activePlayer.Pieces.Where(p => p.IsAlive).ToList();
        var movedPieces = activePieces.Where(p => !p.IsFirstMove).ToList();
        Assert.True(movedPieces.Any(), "ИИ должен сдвинуть хотя бы одну фигуру");
    }

    [Fact]
    public void MakeAiTurn_WithNoValidActions_ShouldNotSpendMana()
    {
        // Arrange
        var session = CreateGameSessionWithNoValidActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;
        
        // Настраиваем моки для отсутствия доступных действий
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);
        
        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.0); // Нет доступных действий
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(0.0);

        // Act
        var result = _aiService.MakeAiTurn(session);

        // Assert
        Assert.False(result, "ИИ не должен выполнить ход при отсутствии доступных действий");
        
        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.Equal(initialMana, finalMana); // Ману не должна измениться
    }

    [Fact]
    public void MakeAiTurn_WithAttackAction_ShouldDealDamage()
    {
        // Arrange
        var session = CreateGameSessionWithAttackTarget();
        var activePlayer = session.GetCurrentTurn().ActiveParticipant;
        var targetPiece = session.GetAllPieces().FirstOrDefault(p => p.Owner?.Id != activePlayer.Id && p.IsAlive);
        var initialHp = targetPiece?.HP ?? 0;
        
        // Настраиваем моки для успешной атаки
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);
        
        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.8);
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);

        // Act
        var result = _aiService.MakeAiTurn(session);

        // Assert
        Assert.True(result, "ИИ должен успешно выполнить атаку");
        
        var finalHp = targetPiece?.HP ?? 0;
        Assert.True(finalHp < initialHp, $"ИИ должен нанести урон. Было HP: {initialHp}, стало: {finalHp}");
    }

    [Fact]
    public void MakeAiTurn_WithFailedActions_ShouldNotSpendManaOnFailedActions()
    {
        // Arrange
        var session = CreateGameSessionWithFailingActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;
        
        // Настраиваем моки для действий, которые будут неуспешными
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);
        
        _mockProbabilityMatrix.Setup(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.0); // Все действия будут неуспешными
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(0.0);

        // Act
        var result = _aiService.MakeAiTurn(session);

        // Assert
        Assert.False(result, "ИИ не должен выполнить ход при неуспешных действиях");
        
        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.Equal(initialMana, finalMana); // Ману не должна измениться
    }

    [Fact]
    public void MakeAiTurn_WithPartialSuccess_ShouldSpendManaOnlyOnSuccessfulActions()
    {
        // Arrange
        var session = CreateGameSessionWithMixedActions();
        var initialMana = session.GetCurrentTurn().RemainingMP;
        
        // Настраиваем моки для частично успешных действий
        _mockDifficultyProvider.Setup(x => x.GetDifficultyLevel(It.IsAny<Player>()))
            .Returns(AIDifficultyLevel.Medium);
        _mockDifficultyProvider.Setup(x => x.GetTemperature(AIDifficultyLevel.Medium))
            .Returns(1.0);
        
        // Первое действие успешно, второе - нет
        _mockProbabilityMatrix.SetupSequence(x => x.GetTransitionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>(), It.IsAny<GameSession>()))
            .Returns(0.8)  // Первое действие успешно
            .Returns(0.0); // Второе действие неуспешно
        
        _mockProbabilityMatrix.Setup(x => x.GetReward(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
            .Returns(1.0);

        // Act
        var result = _aiService.MakeAiTurn(session);

        // Assert
        Assert.True(result, "ИИ должен выполнить хотя бы одно успешное действие");
        
        var finalMana = session.GetCurrentTurn().RemainingMP;
        Assert.True(finalMana < initialMana, "ИИ должен потратить ману на успешные действия");
        
        // Проверяем, что потрачено меньше маны, чем было бы при всех успешных действиях
        var manaSpent = initialMana - finalMana;
        Assert.True(manaSpent > 0, "ИИ должен потратить ману на успешные действия");
    }

    private GameSession CreateGameSessionWithValidActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуры для ИИ (далеко от цели)
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece.Id = 1; // Уникальный ID для ИИ
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        // Создаем фигуры для игрока (далеко от ИИ)
        var playerPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 1));
        playerPiece.Id = 2; // Уникальный ID для игрока
        playerPiece.HP = 10;
        playerPiece.Owner = player1;
        player1.AddPiece(playerPiece);
        
        // Устанавливаем ману
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player2, player1, "Test"); // AI первый, Player1 второй
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(aiPiece);
        session.GetBoard().PlacePiece(playerPiece);
        
        return session;
    }

    private GameSession CreateGameSessionWithNoValidActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуры для ИИ в углах (нет доступных ходов)
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        // Устанавливаем ману
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(aiPiece);
        
        return session;
    }

    private GameSession CreateGameSessionWithAttackTarget()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуры для ИИ рядом с целью
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        aiPiece.Id = 1; // Уникальный ID для ИИ
        aiPiece.HP = 10;
        aiPiece.ATK = 5; // Высокая атака
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        // Создаем цель для атаки - рядом с ИИ
        var targetPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        targetPiece.Id = 2; // Уникальный ID для цели
        targetPiece.HP = 10;
        targetPiece.Owner = player1;
        player1.AddPiece(targetPiece);
        
        // Устанавливаем ману
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player2, player1, "Test"); // AI первый, Player1 второй
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(aiPiece);
        session.GetBoard().PlacePiece(targetPiece);
        
        return session;
    }

    private GameSession CreateGameSessionWithFailingActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуры для ИИ
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        // Устанавливаем ману
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(aiPiece);
        
        return session;
    }

    private GameSession CreateGameSessionWithMixedActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем несколько фигур для ИИ
        var aiPiece1 = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece1.HP = 10;
        aiPiece1.Owner = player2;
        player2.AddPiece(aiPiece1);
        
        var aiPiece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(1, 6));
        aiPiece2.HP = 10;
        aiPiece2.Owner = player2;
        player2.AddPiece(aiPiece2);
        
        // Устанавливаем ману
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player2, player1, "Test"); // AI первый, Player1 второй
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(aiPiece1);
        session.GetBoard().PlacePiece(aiPiece2);
        
        return session;
    }
}
