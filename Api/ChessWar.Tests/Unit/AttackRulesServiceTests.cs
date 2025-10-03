using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Services.GameLogic;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;
using FluentAssertions;

namespace ChessWar.Tests.Unit;

public class AttackRulesServiceTests
{
    private readonly IAttackRulesService _attackRulesService;

    public AttackRulesServiceTests()
    {
        _attackRulesService = new AttackRulesService();
    }

    #region CalculateChebyshevDistance Tests

    [Theory]
    [InlineData(0, 0, 0, 0, 0)] // Same position
    [InlineData(0, 0, 1, 0, 1)] // Horizontal
    [InlineData(0, 0, 0, 1, 1)] // Vertical
    [InlineData(0, 0, 1, 1, 1)] // Diagonal
    [InlineData(0, 0, 2, 1, 2)] // Mixed
    [InlineData(0, 0, 3, 4, 4)] // Max of both
    public void CalculateChebyshevDistance_WithVariousPositions_ShouldReturnCorrectDistance(
        int fromX, int fromY, int toX, int toY, int expectedDistance)
    {
        var from = new Position(fromX, fromY);
        var to = new Position(toX, toY);

        var result = _attackRulesService.CalculateChebyshevDistance(from, to);

        result.Should().Be(expectedDistance);
    }

    #endregion

    #region AvailableAttacks Filtering (Only Enemies)

    [Fact]
    public void GetAvailableAttacks_King_ShouldReturnOnlyEnemyCells()
    {
        var king = new Piece(PieceType.King, Team.Elves, new Position(4, 1)) { HP = 10 };
        var ally1 = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 1)) { HP = 10 };
        var ally2 = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 2)) { HP = 10 };
        var enemy1 = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 1)) { HP = 10 };
        var enemy2 = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 0)) { HP = 10 };
        var pieces = new List<Piece> { king, ally1, ally2, enemy1, enemy2 };

        var result = _attackRulesService.GetAvailableAttacks(king, pieces);

        result.Should().BeEquivalentTo(new[] { new Position(5, 1), new Position(3, 0) });
    }

    [Fact]
    public void GetAvailableAttacks_Pawn_Elves_ShouldAttackForwardStraightOrDiagonalsWithEnemies()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 1)) { HP = 10 };
        var allyDiag = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 2)) { HP = 10 };
        var enemyDiag = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 2)) { HP = 10 };
        var enemyAhead = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 2)) { HP = 10 };
        var enemySide = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 1)) { HP = 10 };
        var pieces = new List<Piece> { pawn, allyDiag, enemyDiag, enemyAhead, enemySide };

        var result = _attackRulesService.GetAvailableAttacks(pawn, pieces);

        result.Should().BeEquivalentTo(new[] { new Position(5, 2), new Position(4, 2) });
    }

    [Fact]
    public void GetAvailableAttacks_Pawn_Orcs_ShouldAttackForwardStraightOrDiagonalsWithEnemies()
    {
        var pawn = new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 6)) { HP = 10 };
        var allyDiag = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 5)) { HP = 10 };
        var enemyDiag = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 5)) { HP = 10 };
        var enemyAhead = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 5)) { HP = 10 };
        var enemySide = new Piece(PieceType.Pawn, Team.Elves, new Position(5, 6)) { HP = 10 };
        var pieces = new List<Piece> { pawn, allyDiag, enemyDiag, enemyAhead, enemySide };

        var result = _attackRulesService.GetAvailableAttacks(pawn, pieces);

        result.Should().BeEquivalentTo(new[] { new Position(3, 5), new Position(4, 5) });
    }

    [Fact]
    public void GetAvailableAttacks_Knight_ShouldReturnOnlyEnemiesWithinRadius1()
    {
        var knight = new Piece(PieceType.Knight, Team.Elves, new Position(4, 4)) { HP = 10 };
        var ally = new Piece(PieceType.Pawn, Team.Elves, new Position(5, 5)) { HP = 10 };
        var enemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 3)) { HP = 10 };
        var farEnemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(6, 6)) { HP = 10 }; // вне радиуса 1
        var pieces = new List<Piece> { knight, ally, enemy, farEnemy };

        var result = _attackRulesService.GetAvailableAttacks(knight, pieces);

        result.Should().BeEquivalentTo(new[] { new Position(3, 3) });
    }

    [Fact]
    public void GetAvailableAttacks_Bishop_ShouldRespectRadius4_Los_AndEnemiesOnly()
    {
        var bishop = new Piece(PieceType.Bishop, Team.Elves, new Position(2, 2)) { HP = 10 };
        var enemyNear = new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 3)) { HP = 10 }; // в радиусе 1
        var enemyFar = new Piece(PieceType.Pawn, Team.Orcs, new Position(6, 6)) { HP = 10 }; // вне радиуса 4
        var blocker = new Piece(PieceType.Pawn, Team.Elves, new Position(4, 4)) { HP = 10 }; // блокирует (5,5)
        var enemyBlocked = new Piece(PieceType.Pawn, Team.Orcs, new Position(5, 5)) { HP = 10 }; // за блоком
        var emptyTarget = new Position(1, 3); // пустая диагональ
        var pieces = new List<Piece> { bishop, enemyNear, enemyFar, blocker, enemyBlocked };

        var result = _attackRulesService.GetAvailableAttacks(bishop, pieces);

        result.Should().BeEquivalentTo(new[] { new Position(3, 3) });
        result.Should().NotContain(emptyTarget);
    }

    [Fact]
    public void GetAvailableAttacks_Rook_ShouldRespectRadius8_Los_AndEnemiesOnly()
    {
        var rook = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0)) { HP = 10 };
        var enemyAlong = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 3)) { HP = 10 };
        var allyBlock = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 2)) { HP = 10 }; // блокирует
        var farEnemy = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 7)) { HP = 10 }; // за блоком
        var pieces = new List<Piece> { rook, enemyAlong, allyBlock, farEnemy };

        var result = _attackRulesService.GetAvailableAttacks(rook, pieces);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetAvailableAttacks_Queen_ShouldRespectRadius3_Los_AndEnemiesOnly()
    {
        var queen = new Piece(PieceType.Queen, Team.Elves, new Position(4, 4)) { HP = 10 };
        var enemyDiag = new Piece(PieceType.Pawn, Team.Orcs, new Position(6, 6)) { HP = 10 }; // в радиусе 2
        var allyBlock = new Piece(PieceType.Pawn, Team.Elves, new Position(5, 5)) { HP = 10 }; // блокирует
        var enemyFar = new Piece(PieceType.Pawn, Team.Orcs, new Position(7, 7)) { HP = 10 }; // вне радиуса 3
        var pieces = new List<Piece> { queen, enemyDiag, allyBlock, enemyFar };

        var result = _attackRulesService.GetAvailableAttacks(queen, pieces);

        result.Should().BeEmpty();
    }

    #endregion

    #region IsEnemy Tests

    [Fact]
    public void IsEnemy_WithDifferentTeams_ShouldReturnTrue()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var target = new Piece(PieceType.Pawn, Team.Orcs, new Position(1, 1));
        target.HP = 10;

        var result = _attackRulesService.IsEnemy(attacker, target);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsEnemy_WithSameTeam_ShouldReturnFalse()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var target = new Piece(PieceType.Pawn, Team.Elves, new Position(1, 1));
        target.HP = 10;

        var result = _attackRulesService.IsEnemy(attacker, target);

        result.Should().BeFalse();
    }

    #endregion

    #region Attack Radius Tests (from README)

    [Theory]
    [InlineData(0, 0, 1, 0, false)] // Pawn cannot attack straight
    [InlineData(0, 0, 2, 0, false)] // Pawn cannot attack at distance 2
    [InlineData(0, 0, 0, 1, true)] // Pawn can attack forward straight
    [InlineData(0, 0, 1, 1, true)] // Pawn can attack forward diagonal (Elves)
    public void IsWithinAttackRange_Pawn_ShouldRespectRadius1(int fromX, int fromY, int toX, int toY, bool expected)
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(fromX, fromY));
        attacker.HP = 10;
        var targetPosition = new Position(toX, toY);
        var boardPieces = new List<Piece>(); // Без целей: проверяем только форму допустимой клетки

        var result = _attackRulesService.IsWithinAttackRange(attacker, targetPosition, boardPieces);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, true)] // Knight can attack at distance 1
    [InlineData(0, 0, 2, 0, false)] // Knight cannot attack at distance 2
    [InlineData(0, 0, 0, 1, true)] // Knight can attack at distance 1
    [InlineData(0, 0, 1, 1, true)] // Knight can attack diagonally at distance 1
    public void IsWithinAttackRange_Knight_ShouldRespectRadius1(int fromX, int fromY, int toX, int toY, bool expected)
    {
        var attacker = new Piece(PieceType.Knight, Team.Elves, new Position(fromX, fromY));
        attacker.HP = 10;
        var targetPosition = new Position(toX, toY);
        var boardPieces = new List<Piece>(); // Пустой список - тестируем только радиус!

        var result = _attackRulesService.IsWithinAttackRange(attacker, targetPosition, boardPieces);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, true)] // Bishop can attack at distance 1
    [InlineData(0, 0, 2, 0, true)] // Bishop can attack at distance 2
    [InlineData(0, 0, 3, 0, true)] // Bishop can attack at distance 3
    [InlineData(0, 0, 4, 0, true)] // Bishop can attack at distance 4
    [InlineData(0, 0, 5, 0, false)] // Bishop cannot attack at distance 5
    public void IsWithinAttackRange_Bishop_ShouldRespectRadius4(int fromX, int fromY, int toX, int toY, bool expected)
    {
        var attacker = new Piece(PieceType.Bishop, Team.Elves, new Position(fromX, fromY));
        attacker.HP = 10;
        var targetPosition = new Position(toX, toY);
        var boardPieces = new List<Piece>(); // Пустой список - тестируем только радиус!

        var result = _attackRulesService.IsWithinAttackRange(attacker, targetPosition, boardPieces);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, true)] // Rook can attack at distance 1
    [InlineData(0, 0, 2, 0, true)] // Rook can attack at distance 2
    [InlineData(0, 0, 4, 0, true)] // Rook can attack at distance 4
    [InlineData(0, 0, 7, 0, true)] // Rook can attack at distance 7 (max on board)
    [InlineData(0, 0, 8, 0, false)] // Rook cannot attack at distance 8 (out of bounds)
    public void IsWithinAttackRange_Rook_ShouldRespectRadius8(int fromX, int fromY, int toX, int toY, bool expected)
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(fromX, fromY));
        attacker.HP = 10;
        var targetPosition = new Position(toX, toY);
        var boardPieces = new List<Piece>(); // Пустой список - тестируем только радиус!

        var result = _attackRulesService.IsWithinAttackRange(attacker, targetPosition, boardPieces);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, true)] // Queen can attack at distance 1
    [InlineData(0, 0, 2, 0, true)] // Queen can attack at distance 2
    [InlineData(0, 0, 3, 0, true)] // Queen can attack at distance 3
    [InlineData(0, 0, 4, 0, false)] // Queen cannot attack at distance 4
    public void IsWithinAttackRange_Queen_ShouldRespectRadius3(int fromX, int fromY, int toX, int toY, bool expected)
    {
        var attacker = new Piece(PieceType.Queen, Team.Elves, new Position(fromX, fromY));
        attacker.HP = 10;
        var targetPosition = new Position(toX, toY);
        var boardPieces = new List<Piece>(); // Пустой список - тестируем только радиус!

        var result = _attackRulesService.IsWithinAttackRange(attacker, targetPosition, boardPieces);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0, 0, 1, 0, true)] // King can attack at distance 1
    [InlineData(0, 0, 2, 0, false)] // King cannot attack at distance 2
    [InlineData(0, 0, 0, 1, true)] // King can attack at distance 1
    [InlineData(0, 0, 1, 1, true)] // King can attack diagonally at distance 1
    public void IsWithinAttackRange_King_ShouldRespectRadius1(int fromX, int fromY, int toX, int toY, bool expected)
    {
        var attacker = new Piece(PieceType.King, Team.Elves, new Position(fromX, fromY));
        attacker.HP = 10;
        var targetPosition = new Position(toX, toY);
        var boardPieces = new List<Piece>(); // Пустой список - тестируем только радиус!

        var result = _attackRulesService.IsWithinAttackRange(attacker, targetPosition, boardPieces);

        result.Should().Be(expected);
    }

    #endregion

    #region Obstacle Tests

    [Fact]
    public void CanAttack_WithObstacleInPath_ShouldReturnFalse()
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(3, 0);
        var obstacle = new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 0));
        obstacle.HP = 10;
        var boardPieces = new List<Piece> { obstacle };

        var result = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        result.Should().BeFalse();
    }

    [Fact]
    public void CanAttack_WithNoObstacleInPath_ShouldReturnTrue()
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(3, 0);
        var obstacle = new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 1)); // Different row
        obstacle.HP = 10;
        var target = new Piece(PieceType.Pawn, Team.Orcs, targetPosition); // Цель в позиции атаки
        target.HP = 10;
        var boardPieces = new List<Piece> { obstacle, target };

        var result = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        result.Should().BeTrue();
    }

    [Fact]
    public void CanAttack_WithObstacleAtTarget_ShouldReturnTrue()
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(2, 0);
        var target = new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 0)); // At target position
        target.HP = 10;
        var boardPieces = new List<Piece> { target };

        var result = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        result.Should().BeTrue();
    }

    #endregion

    #region HasValidTarget Tests

    [Fact]
    public void HasValidTarget_ShouldReturnFalse_WhenNoTarget()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(1, 1); // диагональ вперёд для Elves
        var boardPieces = new List<Piece>(); // Пустой список

        var result = _attackRulesService.HasValidTarget(attacker, targetPosition, boardPieces);

        result.Should().BeFalse(); // Нет цели
    }

    [Fact]
    public void HasValidTarget_ShouldReturnFalse_WhenTargetIsDead()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10; // Устанавливаем HP для живого атакующего
        var targetPosition = new Position(1, 0);
        var deadTarget = new Piece(PieceType.Pawn, Team.Orcs, targetPosition);
        deadTarget.HP = 10; // Устанавливаем HP
        TestHelpers.TakeDamage(deadTarget, 100);
        var boardPieces = new List<Piece> { deadTarget };

        var result = _attackRulesService.HasValidTarget(attacker, targetPosition, boardPieces);

        result.Should().BeFalse(); // Мёртвая цель
    }

    [Fact]
    public void HasValidTarget_ShouldReturnFalse_WhenTargetIsAlly()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10; // Устанавливаем HP для живого атакующего
        var targetPosition = new Position(1, 0);
        var allyTarget = new Piece(PieceType.Pawn, Team.Elves, targetPosition); // Союзник
        allyTarget.HP = 10; // Устанавливаем HP для живой цели
        var boardPieces = new List<Piece> { allyTarget };

        var result = _attackRulesService.HasValidTarget(attacker, targetPosition, boardPieces);

        result.Should().BeFalse(); // Союзник
    }

    [Fact]
    public void HasValidTarget_ShouldReturnTrue_WhenValidEnemyTarget()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(1, 0);
        var enemyTarget = new Piece(PieceType.Pawn, Team.Orcs, targetPosition); // Враг
        enemyTarget.HP = 10;
        var boardPieces = new List<Piece> { enemyTarget };

        var result = _attackRulesService.HasValidTarget(attacker, targetPosition, boardPieces);

        result.Should().BeTrue(); // Валидная вражеская цель
    }

    #endregion

    #region CanAttack Integration Tests

    [Fact]
    public void CanAttack_ShouldReturnTrue_WhenValidAttack()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10; // Устанавливаем HP для живого атакующего
        var targetPosition = new Position(1, 1); // диагональ вперёд для Elves
        var enemyTarget = new Piece(PieceType.Pawn, Team.Orcs, targetPosition);
        enemyTarget.HP = 10; // Устанавливаем HP для живой цели
        var boardPieces = new List<Piece> { attacker, enemyTarget };

        var result = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        result.Should().BeTrue(); // Полная атака должна работать
    }

    [Fact]
    public void CanAttack_ShouldReturnFalse_WhenNoTarget()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10; // Устанавливаем HP для живого атакующего
        var targetPosition = new Position(1, 0);
        var boardPieces = new List<Piece>(); // Пустой список

        var result = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        result.Should().BeFalse(); // Нет цели
    }

    [Fact]
    public void CanAttack_ShouldReturnFalse_WhenOutOfRange()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(0, 0));
        attacker.HP = 10; // Устанавливаем HP для живого атакующего
        var targetPosition = new Position(3, 0); // Слишком далеко для пешки
        var enemyTarget = new Piece(PieceType.Pawn, Team.Orcs, targetPosition);
        enemyTarget.HP = 10; // Устанавливаем HP для живой цели
        var boardPieces = new List<Piece> { enemyTarget };

        var result = _attackRulesService.CanAttack(attacker, targetPosition, boardPieces);

        result.Should().BeFalse(); // Вне радиуса
    }

    #endregion

    #region GetAttackablePositions Tests

    [Fact]
    public void GetAttackablePositions_Pawn_ShouldReturnCorrectPositions()
    {
        var attacker = new Piece(PieceType.Pawn, Team.Elves, new Position(3, 3));
        attacker.HP = 10;

        var boardPieces = new List<Piece>
        {
            new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 2)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 3)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 4)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 2)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(3, 4)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 2)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 3)) { HP = 10 },
            new Piece(PieceType.Pawn, Team.Orcs, new Position(4, 4)) { HP = 10 }
        };

        var result = _attackRulesService.GetAttackablePositions(attacker, boardPieces);

        result.Should().BeEquivalentTo(new[] { new Position(4, 4), new Position(3, 4), new Position(2, 4) });
    }

    [Fact]
    public void GetAttackablePositions_WithObstacles_ShouldExcludeBlockedPositions()
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var obstacle = new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 0));
        obstacle.HP = 10;
        var boardPieces = new List<Piece> { obstacle };

        var result = _attackRulesService.GetAttackablePositions(attacker, boardPieces);

        result.Should().NotContain(new Position(3, 0)); // Blocked by obstacle
        result.Should().NotContain(new Position(4, 0)); // Blocked by obstacle
        result.Should().Contain(new Position(2, 0)); // Target position itself (enemy)
        result.Should().NotContain(new Position(1, 0)); // Before obstacle — пустая
    }

    #endregion

    #region IsPathClear Tests

    [Fact]
    public void IsPathClear_WithClearPath_ShouldReturnTrue()
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(3, 0);
        var boardPieces = new List<Piece>();

        var result = _attackRulesService.IsPathClear(attacker, targetPosition, boardPieces);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsPathClear_WithObstacle_ShouldReturnFalse()
    {
        var attacker = new Piece(PieceType.Rook, Team.Elves, new Position(0, 0));
        attacker.HP = 10;
        var targetPosition = new Position(3, 0);
        var obstacle = new Piece(PieceType.Pawn, Team.Orcs, new Position(2, 0));
        obstacle.HP = 10;
        var boardPieces = new List<Piece> { obstacle };

        var result = _attackRulesService.IsPathClear(attacker, targetPosition, boardPieces);

        result.Should().BeFalse();
    }

    #endregion


    #region Helper Methods

    private Piece CreateTestPiece(string id, PieceType type, Team team, Position position)
    {
        var owner = new Player($"Player_{team}", new List<Piece>());
        var piece = new Piece(id, type, team, position, owner);
        return piece;
    }

    #endregion
}
