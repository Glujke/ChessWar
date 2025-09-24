using ChessWar.Domain.Entities;
using ChessWar.Domain.Services.AI;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.ValueObjects;
using ChessWar.Domain.Enums;
using ChessWar.Infrastructure.Services.AI;
using ChessWar.Application.Services.Pieces;
using ChessWar.Tests.Unit;
using Moq;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Integration.AI;

/// <summary>
/// Интеграционные тесты для проверки реального поведения ИИ
/// </summary>
public class AIRealBehaviorTests
{
    [Fact]
    public void AI_ShouldActuallyMovePiecesWhenSpendingMana()
    {
        // Arrange
        var session = CreateRealGameSession();
        var aiService = CreateRealAIService();
        
        var initialPositions = GetPiecePositions(session.Player1); // AI теперь Player1
        var initialMana = session.GetCurrentTurn().RemainingMP;
        
        // Act
        var result = aiService.MakeAiTurn(session);
        
        // Assert
        Assert.True(result, "ИИ должен успешно выполнить ход");
        
        var finalPositions = GetPiecePositions(session.Player1); // AI теперь Player1
        
        // Проверяем, что ману потрачена у AI (Player1)
        var aiMana = session.Player1.MP;
        Assert.True(aiMana < 50, $"ИИ должен потратить ману. Было: 50, стало: {aiMana}");
        
        // Проверяем, что фигуры действительно сдвинулись
        var movedPieces = CountMovedPieces(initialPositions, finalPositions);
        Assert.True(movedPieces > 0, $"ИИ должен сдвинуть фигуры. Сдвинуто: {movedPieces}");
        
        // Проверяем, что потраченная мана соответствует количеству действий
        var manaSpent = 50 - aiMana;
        Assert.True(manaSpent > 0, "ИИ должен потратить ману на действия");
    }

    [Fact]
    public void AI_ShouldActuallyDealDamageWhenAttacking()
    {
        // Arrange
        var session = CreateGameSessionWithAttackTarget();
        var aiService = CreateRealAIService();
        
        var targetPiece = session.Player1.Pieces.First(p => p.IsAlive);
        var initialHp = targetPiece.HP;
        var initialMana = session.Player2.MP; // Проверяем ману у игрока
        
        // Act
        var result = aiService.MakeAiTurn(session);
        
        // Assert
        Assert.True(result, "ИИ должен успешно выполнить атаку");
        
        var finalHp = targetPiece.HP;
        var finalMana = session.Player2.MP; // Проверяем ману у игрока
        
        // Проверяем, что ману потрачена
        Assert.True(finalMana < initialMana, "ИИ должен потратить ману на атаку");
        
        // Проверяем, что урон нанесен
        Assert.True(finalHp < initialHp, $"ИИ должен нанести урон. Было HP: {initialHp}, стало: {finalHp}");
    }

    [Fact]
    public void AI_ShouldNotSpendManaOnImpossibleActions()
    {
        // Arrange
        var session = CreateGameSessionWithNoValidActions();
        var aiService = CreateRealAIService();
        
        var initialMana = session.GetCurrentTurn().RemainingMP;
        var initialPositions = GetPiecePositions(session.Player2);
        
        // Act
        var result = aiService.MakeAiTurn(session);
        
        // Assert
        var finalMana = session.GetCurrentTurn().RemainingMP;
        var finalPositions = GetPiecePositions(session.Player2);
        
        // ИИ не должен тратить ману на невозможные действия
        Assert.Equal(initialMana, finalMana); // Ману не должна измениться
        Assert.Equal(initialPositions, finalPositions); // Позиции не должны измениться
        
        // ИИ может вернуть false или true, но не должен тратить ману
        if (result)
        {
            // Если ИИ вернул true, он должен был сделать хотя бы одно действие
            var movedPieces = CountMovedPieces(initialPositions, finalPositions);
            Assert.True(movedPieces > 0, "Если ИИ вернул true, он должен сдвинуть фигуры");
        }
    }

    [Fact]
    public void AI_ShouldUseAbilitiesWhenAvailable()
    {
        // Arrange
        var session = CreateGameSessionWithAbilities();
        var aiService = CreateRealAIService();
        
        var initialMana = session.Player2.MP; // Проверяем ману у игрока
        var pieceWithAbility = session.Player2.Pieces.First(p => p.AbilityCooldowns.ContainsKey("__AuraBuff"));
        var initialCooldown = pieceWithAbility.AbilityCooldowns["__AuraBuff"];
        
        // Act
        var result = aiService.MakeAiTurn(session);
        
        // Assert
        Assert.True(result, "ИИ должен успешно выполнить ход");
        
        var finalMana = session.Player2.MP; // Проверяем ману у игрока
        var finalCooldown = pieceWithAbility.AbilityCooldowns["__AuraBuff"];
        
        // Проверяем, что ману потрачена
        Assert.True(finalMana < initialMana, "ИИ должен потратить ману на способности");
        
        // Проверяем, что способность использована (кулдаун изменился)
        Assert.NotEqual(initialCooldown, finalCooldown);
    }

    [Fact]
    public void AI_ShouldRespectManaLimits()
    {
        // Arrange
        var session = CreateGameSessionWithLimitedMana();
        var aiService = CreateRealAIService();
        
        var initialMana = session.GetCurrentTurn().RemainingMP;
        
        // Act
        var result = aiService.MakeAiTurn(session);
        
        // Assert
        var finalMana = session.GetCurrentTurn().RemainingMP;
        
        // ИИ не должен потратить больше маны, чем доступно
        Assert.True(finalMana >= 0, "ИИ не должен потратить больше маны, чем доступно");
        
        if (result)
        {
            // Если ИИ выполнил ход, он должен потратить часть маны
            Assert.True(finalMana < initialMana, "ИИ должен потратить часть маны");
        }
    }

    private GameSession CreateRealGameSession()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем генератор ID
        var idGenerator = new PieceIdGenerator();
        
        // Создаем фигуры для ИИ в центре доски
        var aiPiece1 = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        aiPiece1.Id = idGenerator.GetNextId();
        aiPiece1.HP = 10;
        aiPiece1.Owner = player2;
        player2.AddPiece(aiPiece1);
        Console.WriteLine($"[CreateRealGameSession] AI Piece 1: ID={aiPiece1.Id}, Owner={aiPiece1.Owner?.Id}, Team={aiPiece1.Team}");
        
        var aiPiece2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 6));
        aiPiece2.Id = idGenerator.GetNextId();
        aiPiece2.HP = 10;
        aiPiece2.Owner = player2;
        player2.AddPiece(aiPiece2);
        Console.WriteLine($"[CreateRealGameSession] AI Piece 2: ID={aiPiece2.Id}, Owner={aiPiece2.Owner?.Id}, Team={aiPiece2.Team}");
        
        // Создаем фигуры для игрока
        var playerPiece1 = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1));
        playerPiece1.Id = idGenerator.GetNextId();
        playerPiece1.HP = 10;
        playerPiece1.Owner = player1;
        player1.AddPiece(playerPiece1);
        Console.WriteLine($"[CreateRealGameSession] Player Piece 1: ID={playerPiece1.Id}, Owner={playerPiece1.Owner?.Id}, Team={playerPiece1.Team}");
        
        var playerPiece2 = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 1));
        playerPiece2.Id = idGenerator.GetNextId();
        playerPiece2.HP = 10;
        playerPiece2.Owner = player1;
        player1.AddPiece(playerPiece2);
        Console.WriteLine($"[CreateRealGameSession] Player Piece 2: ID={playerPiece2.Id}, Owner={playerPiece2.Owner?.Id}, Team={playerPiece2.Team}");
        
        // Устанавливаем ману
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player2, player1, "Test"); // AI первый, Player1 второй
        session.StartGame();
        
        // Размещаем фигуры на доске
        session.GetBoard().PlacePiece(aiPiece1);
        session.GetBoard().PlacePiece(aiPiece2);
        session.GetBoard().PlacePiece(playerPiece1);
        session.GetBoard().PlacePiece(playerPiece2);
        
        return session;
    }

    private GameSession CreateGameSessionWithAttackTarget()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуры ИИ рядом с целью
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 6));
        aiPiece.Id = 1; // Уникальный ID для фигуры ИИ
        aiPiece.HP = 10;
        aiPiece.ATK = 5;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        // Создаем цель для атаки
        var targetPiece = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 5));
        targetPiece.Id = 2; // Уникальный ID для цели
        targetPiece.HP = 10;
        targetPiece.Owner = player1;
        player1.AddPiece(targetPiece);
        
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);
        session.GetBoard().PlacePiece(targetPiece);
        
        // Устанавливаем активного игрока как Player2 (ИИ)
        var newTurn = new Turn(1, player2);
        session.SetCurrentTurn(newTurn);
        
        return session;
    }

    private GameSession CreateGameSessionWithNoValidActions()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуры ИИ в углах (нет доступных ходов)
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);
        
        return session;
    }

    private GameSession CreateGameSessionWithAbilities()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        // Создаем фигуру ИИ со способностью
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        aiPiece.AbilityCooldowns["__AuraBuff"] = 0; // Способность доступна
        player2.AddPiece(aiPiece);
        
        player1.SetMana(50, 50);
        player2.SetMana(50, 50);
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);
        
        // Устанавливаем активного игрока как Player2 (ИИ)
        // Создаем новый ход с Player2 как активным игроком
        var newTurn = new Turn(1, player2);
        session.SetCurrentTurn(newTurn);
        
        return session;
    }

    private GameSession CreateGameSessionWithLimitedMana()
    {
        var player1 = new Player("Player 1", new List<Piece>());
        var player2 = new Player("AI", new List<Piece>());
        
        var aiPiece = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 6));
        aiPiece.HP = 10;
        aiPiece.Owner = player2;
        player2.AddPiece(aiPiece);
        
        player1.SetMana(50, 50);
        player2.SetMana(5, 5); // Ограниченная мана
        
        var session = new GameSession(player1, player2, "Test");
        session.StartGame();
        session.GetBoard().PlacePiece(aiPiece);
        
        return session;
    }

    private ProbabilisticAIService CreateRealAIService()
    {
        var evaluator = new GameStateEvaluator();
        var probabilityMatrix = new ChessWarProbabilityMatrix(evaluator);
        var difficultyProvider = new AIDifficultyProvider();
        var logger = Mock.Of<ILogger<ProbabilisticAIService>>();
        
        // Создаем мок TurnService, который реально выполняет действия
        var mockTurnService = new Mock<ITurnService>();
        
        // Создаем мок AbilityService
        var mockAbilityService = new Mock<IAbilityService>();
        mockAbilityService.Setup(x => x.UseAbility(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
            .Callback<Piece, string, Position, List<Piece>>((piece, abilityName, target, allPieces) => {
                Console.WriteLine($"[MockAbilityService] UseAbility callback called for piece {piece.Id} with ability {abilityName}");
                // Устанавливаем кулдаун для способности
                piece.AbilityCooldowns[abilityName] = 3; // 3 хода кулдауна
                
                // Тратим ману на способность (обычно 1-2 маны)
                var manaCost = 2; // Стоимость способности
                piece.Owner?.Spend(manaCost);
            })
            .Returns(true);
        mockTurnService.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) => {
                Console.WriteLine($"[MockTurnService] ExecuteMove callback called for piece {piece.Id} to ({position.X},{position.Y})");
                Console.WriteLine($"[MockTurnService] Piece owner: {piece.Owner?.Id}, Team: {piece.Team}");
                Console.WriteLine($"[MockTurnService] Turn active participant: {turn.ActiveParticipant.Id}");
                Console.WriteLine($"[MockTurnService] Before: Turn.RemainingMP={turn.RemainingMP}, Player.MP={turn.ActiveParticipant.MP}");
                // Тратим ману за движение
                turn.SpendMP(1);
                turn.ActiveParticipant.Spend(1);
                // Обновляем позицию фигуры напрямую
                Console.WriteLine($"[MockTurnService] Before move: piece.Position={piece.Position}");
                piece.Position = position;
                Console.WriteLine($"[MockTurnService] After move: piece.Position={piece.Position}");
                // Обновляем позицию на доске
                session.GetBoard().MovePiece(piece, position);
                // Обновляем оставшуюся ману из состояния игрока
                turn.UpdateRemainingMP();
                Console.WriteLine($"[MockTurnService] After: Turn.RemainingMP={turn.RemainingMP}, Player.MP={turn.ActiveParticipant.MP}");
            })
            .Returns(true);
        mockTurnService.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
            .Callback<GameSession, Turn, Piece, Position>((session, turn, piece, position) => {
                // Тратим ману за атаку
                turn.SpendMP(2);
                turn.ActiveParticipant.Spend(2);
                
                // Находим цель и наносим урон
                var target = session.GetAllPieces().FirstOrDefault(p => p.Position.Equals(position) && p.Owner?.Id != piece.Owner?.Id);
                if (target != null)
                {
                    TestHelpers.TakeDamage(target, piece.ATK);
                }
                
                // Обновляем оставшуюся ману из состояния игрока
                turn.UpdateRemainingMP();
            })
            .Returns(true);
        
        return new ProbabilisticAIService(probabilityMatrix, evaluator, difficultyProvider, mockTurnService.Object, mockAbilityService.Object, logger);
    }

    private Dictionary<int, Position> GetPiecePositions(Player player)
    {
        return player.Pieces
            .Where(p => p.IsAlive)
            .Select((p, index) => new { Piece = p, Index = index })
            .ToDictionary(x => x.Index, x => x.Piece.Position);
    }

    private int CountMovedPieces(Dictionary<int, Position> initial, Dictionary<int, Position> final)
    {
        return initial.Count(kvp => 
            final.ContainsKey(kvp.Key) && 
            final[kvp.Key] != kvp.Value);
    }
}
