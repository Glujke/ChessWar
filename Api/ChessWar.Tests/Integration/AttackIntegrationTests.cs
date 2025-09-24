using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using ChessWar.Domain.Enums;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

/// <summary>
/// Интеграционные тесты для полного цикла атак: подход к врагу, атака, убийство, получение опыта
/// </summary>
public class AttackIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public AttackIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Attack_ShouldKillEnemy_AndGiveExperience_WhenAttackingAdjacentPawn()
    {
        // Arrange: создаём игровую сессию
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();
        session!.Status.Should().Be(GameStatus.Active);

        // Получаем начальное состояние
        var initialState = await GetGameState(session.Id);
        var attacker = initialState.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var target = initialState.Player2.Pieces.First(p => p.Type == PieceType.Pawn);
        
        // Используем реальные значения HP фигур из игровой сессии
        
        // Запоминаем начальные значения
        var initialAttackerXP = attacker.XP;
        var initialAttackerPosition = attacker.Position;
        var targetPosition = target.Position;

        // Act: выполняем полный цикл атаки
        
        // 1. Подходим к цели за несколько ходов (с 0,1 на 0,5) для возможности диагональной атаки
        // Первый ход: с 0,1 на 0,3 (первый ход пешки - на 2 клетки)
        var moveAction1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 3 }
        };
        
        var moveResponse1 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction1);
        moveResponse1.IsSuccessStatusCode.Should().BeTrue("Первый ход пешки должен быть успешным");
        
        // Второй ход: с 0,3 на 0,4 (обычный ход пешки - на 1 клетку)
        var moveAction2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 4 }
        };
        
        var moveResponse2 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction2);
        moveResponse2.IsSuccessStatusCode.Should().BeTrue("Второй ход пешки должен быть успешным");
        
        // Третий ход: с 0,4 на 0,5
        var moveAction3 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 5 }
        };
        
        var moveResponse3 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction3);
        moveResponse3.IsSuccessStatusCode.Should().BeTrue("Третий ход пешки должен быть успешным");

        // 2. Используем способность "Прорыв" (3 урона) - атакуем диагонально врага на позиции (1,6)
        var breakthroughAction = new ExecuteActionDto
        {
            Type = "Ability",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 1, Y = 6 },
            Description = "Breakthrough"
        };
        
        var breakthroughResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", breakthroughAction);
        breakthroughResponse.IsSuccessStatusCode.Should().BeTrue("Способность Прорыв должна быть успешной");

        // 3. Атакуем несколько раз до убийства цели
        var attackTargetPosition = new PositionDto { X = 1, Y = 6 };
        var attackCount = 0;
        var maxAttacks = 10; // Защита от бесконечного цикла
        
        while (attackCount < maxAttacks)
        {
            var attackAction = new ExecuteActionDto
            {
                Type = "Attack",
                PieceId = attacker.Id.ToString(),
                TargetPosition = attackTargetPosition
            };
            
            var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);
            attackResponse.IsSuccessStatusCode.Should().BeTrue($"Атака {attackCount + 1} должна быть успешной");
            
            // Проверяем, убита ли цель
            var currentState = await GetGameState(session.Id);
            var targetPiece = currentState.Player2.Pieces.FirstOrDefault(p => p.Position.X == 1 && p.Position.Y == 6);
            
            if (targetPiece == null || targetPiece.HP <= 0)
            {
                break; // Цель убита
            }
            
            attackCount++;
        }
        
        // Проверяем, что цель была убита
        attackCount.Should().BeLessThan(maxAttacks, "Цель должна быть убита за разумное количество атак");

        // 4. Завершаем ход
        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{session.Id}/turn/end", null);
        endTurnResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Завершение хода должно быть успешным");

        // Assert: проверяем результаты
        var finalState = await GetGameState(session.Id);
        var finalAttacker = finalState.Player1.Pieces.First(p => p.Id == attacker.Id);
        
        // Ищем пешку на позиции (1,6) - ту, которую мы атаковали
        var attackedPawn = finalState.Player2.Pieces.FirstOrDefault(p => p.Position.X == 1 && p.Position.Y == 6);

        // Отладочная информация
        Console.WriteLine($"Final attacker XP: {finalAttacker.XP}, Initial: {initialAttackerXP}");
        Console.WriteLine($"Attacked pawn HP: {attackedPawn?.HP ?? -1}");
        Console.WriteLine($"Attacked pawn position: {attackedPawn?.Position}");

        // Атакующий должен получить опыт за убийство
        finalAttacker.XP.Should().BeGreaterThan(initialAttackerXP, "Атакующий должен получить опыт за убийство");

        // Цель должна быть убита (HP = 0 или удалена с доски)
        if (attackedPawn != null)
        {
            attackedPawn.HP.Should().Be(0, "Атакованная пешка должна быть убита");
        }
        else
        {
            // Если фигура удалена с доски, это тоже нормально
            finalState.Player2.Pieces.Should().NotContain(p => p.Position.X == 1 && p.Position.Y == 6, "Убитая фигура должна быть удалена с доски");
        }

        // Проверяем, что атакующий получил опыт за убийство (основная цель теста)
        // Примечание: проверка маны убрана, так как между ходами происходит регенерация
    }

    [Fact]
    public async Task Attack_ShouldFail_WhenNotEnoughMana()
    {
        // Arrange: создаём игровую сессию
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        // Получаем состояние
        var state = await GetGameState(session!.Id);
        var attacker = state.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var target = state.Player2.Pieces.First(p => p.Type == PieceType.Pawn);

        // Тратим всю ману игрока
        var initialMP = state.Player1.MP;
        while (state.Player1.MP > 0)
        {
            var moveAction = new ExecuteActionDto
            {
                Type = "Move",
                PieceId = attacker.Id.ToString(),
                TargetPosition = new PositionDto { X = attacker.Position.X + 1, Y = attacker.Position.Y }
            };
            
            var moveResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction);
            if (!moveResponse.IsSuccessStatusCode)
                break;
                
            state = await GetGameState(session.Id);
        }

        // Act: пытаемся атаковать без маны
        var attackAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = target.Position.X, Y = target.Position.Y }
        };
        
        var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);

        if (!attackResponse.IsSuccessStatusCode)
        {
            var errorContent = await attackResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Attack without mana failed: {attackResponse.StatusCode} - {errorContent}");
        }

        // Assert: атака должна провалиться
        attackResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Атака без маны должна провалиться");
    }

    [Fact]
    public async Task Attack_ShouldFail_WhenTargetIsAlly()
    {
        // Arrange: создаём игровую сессию
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        // Получаем состояние
        var state = await GetGameState(session!.Id);
        var attacker = state.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var ally = state.Player1.Pieces.First(p => p.Id != attacker.Id);

        // Act: пытаемся атаковать союзника
        var attackAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = ally.Position.X, Y = ally.Position.Y }
        };
        
        var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);

        // Assert: атака должна провалиться
        attackResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Атака союзника должна провалиться");
    }

    [Fact]
    public async Task Attack_ShouldFail_WhenTargetIsTooFar()
    {
        // Arrange: создаём игровую сессию
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        
        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        // Получаем состояние
        var state = await GetGameState(session!.Id);
        var attacker = state.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var target = state.Player2.Pieces.First(p => p.Type == PieceType.Pawn);

        // Act: пытаемся атаковать далёкую цель
        var attackAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 7, Y = 7 } // Далёкая позиция
        };
        
        var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);

        // Assert: атака должна провалиться
        attackResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Атака далёкой цели должна провалиться");
    }

    private async Task<GameSessionDto> GetGameState(Guid sessionId)
    {
        var response = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<GameSessionDto>();
        state.Should().NotBeNull();
        return state!;
    }

    private bool IsAdjacent(PositionDto pos1, PositionDto pos2)
    {
        var distance = Math.Max(Math.Abs(pos1.X - pos2.X), Math.Abs(pos1.Y - pos2.Y));
        return distance == 1;
    }


    private PositionDto CalculateValidPawnPosition(PositionDto from, PositionDto to)
    {
        // Для пешки Team.Elves (Player1) - движется вверх (y увеличивается)
        // Для пешки Team.Orcs (Player2) - движется вниз (y уменьшается)
        
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;
        
        // Нормализуем направление
        if (dx != 0) dx = dx > 0 ? 1 : -1;
        if (dy != 0) dy = dy > 0 ? 1 : -1;
        
        // Возвращаем позицию рядом с ЦЕЛЬЮ, но с учетом направления движения пешки
        // Если цель выше (dy > 0), то подходим снизу (dy - 1)
        // Если цель ниже (dy < 0), то подходим сверху (dy + 1)
        return new PositionDto { X = to.X - dx, Y = to.Y - dy };
    }
}
