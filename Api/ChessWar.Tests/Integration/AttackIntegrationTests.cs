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
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();
        session!.Status.Should().Be(GameStatus.Active);

        var initialState = await GetGameState(session.Id);
        var attacker = initialState.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var target = initialState.Player2.Pieces.First(p => p.Type == PieceType.Pawn);


        var initialAttackerXP = attacker.XP;
        var initialAttackerPosition = attacker.Position;
        var targetPosition = target.Position;


        var moveAction1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 3 }
        };

        var moveResponse1 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction1);
        moveResponse1.IsSuccessStatusCode.Should().BeTrue("Первый ход пешки должен быть успешным");

        var moveAction2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 4 }
        };

        var moveResponse2 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction2);
        moveResponse2.IsSuccessStatusCode.Should().BeTrue("Второй ход пешки должен быть успешным");

        var moveAction3 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 0, Y = 5 }
        };

        var moveResponse3 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveAction3);
        moveResponse3.IsSuccessStatusCode.Should().BeTrue("Третий ход пешки должен быть успешным");

        var breakthroughAction = new ExecuteActionDto
        {
            Type = "Ability",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 1, Y = 6 },
            Description = "Breakthrough"
        };

        var breakthroughResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", breakthroughAction);
        breakthroughResponse.IsSuccessStatusCode.Should().BeTrue("Способность Прорыв должна быть успешной");

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

            var currentState = await GetGameState(session.Id);
            var targetPiece = currentState.Player2.Pieces.FirstOrDefault(p => p.Position != null && p.Position.X == 1 && p.Position.Y == 6);

            if (targetPiece == null || targetPiece.HP <= 0)
            {
                break; // Цель убита
            }

            attackCount++;
        }

        attackCount.Should().BeLessThan(maxAttacks, "Цель должна быть убита за разумное количество атак");

        var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{session.Id}/turn/end", null);
        endTurnResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Завершение хода должно быть успешным");

        var finalState = await GetGameState(session.Id);
        var finalAttacker = finalState.Player1.Pieces.First(p => p.Id == attacker.Id);

        var attackedPawn = finalState.Player2.Pieces.FirstOrDefault(p => p.Position != null && p.Position.X == 1 && p.Position.Y == 6);


        finalAttacker.XP.Should().BeGreaterThan(initialAttackerXP, "Атакующий должен получить опыт за убийство");

        if (attackedPawn != null)
        {
            attackedPawn.HP.Should().Be(0, "Атакованная пешка должна быть убита");
        }
        else
        {
            finalState.Player2.Pieces.Should().NotContain(p => p.Position != null && p.Position.X == 1 && p.Position.Y == 6, "Убитая фигура должна быть удалена с доски");
        }

    }

    [Fact]
    public async Task Attack_ShouldFail_WhenNotEnoughMana()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        var state = await GetGameState(session!.Id);
        var attacker = state.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var target = state.Player2.Pieces.First(p => p.Type == PieceType.Pawn);

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
        }

        attackResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Атака без маны должна провалиться");
    }

    [Fact]
    public async Task Attack_ShouldFail_WhenTargetIsAlly()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        var state = await GetGameState(session!.Id);
        var attacker = state.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var ally = state.Player1.Pieces.First(p => p.Id != attacker.Id);

        var attackAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = ally.Position.X, Y = ally.Position.Y }
        };

        var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);

        attackResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest, "Атака союзника должна провалиться");
    }

    [Fact]
    public async Task Attack_ShouldFail_WhenTargetIsTooFar()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResponse = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResponse.IsSuccessStatusCode.Should().BeTrue();

        var session = await createResponse.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        var state = await GetGameState(session!.Id);
        var attacker = state.Player1.Pieces.First(p => p.Type == PieceType.Pawn);
        var target = state.Player2.Pieces.First(p => p.Type == PieceType.Pawn);

        var attackAction = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = attacker.Id.ToString(),
            TargetPosition = new PositionDto { X = 7, Y = 7 } // Далёкая позиция
        };

        var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);

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

        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        if (dx != 0) dx = dx > 0 ? 1 : -1;
        if (dy != 0) dy = dy > 0 ? 1 : -1;

        return new PositionDto { X = to.X - dx, Y = to.Y - dy };
    }
}
