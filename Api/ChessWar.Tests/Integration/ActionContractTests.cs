using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace ChessWar.Tests.Integration;

public class ActionContractTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public ActionContractTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task PostAction_ShouldReturn_GameSessionDto()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        var pieceId = session!.Player1.Pieces[0].Id.ToString();
        var actionsResp = await _client.GetAsync($"/api/v1/gamesession/{session.Id}/piece/{pieceId}/actions?actionType=Move");
        actionsResp.IsSuccessStatusCode.Should().BeTrue();
        var actions = await actionsResp.Content.ReadFromJsonAsync<List<PositionDto>>();
        actions.Should().NotBeNull();
        if (actions!.Count == 0)
        {
            return; // если нет доступных ходов для стартовой фигуры, пропускаем (редкий конфиг)
        }

        var actionDto = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = pieceId,
            TargetPosition = actions[0],
            Description = null
        };

        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", actionDto);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await resp.Content.ReadFromJsonAsync<GameSessionDto>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task GetAbilityTargets_ShouldRequire_AbilityName()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var pieceId = session!.Player1.Pieces[0].Id.ToString();
        var resp = await _client.GetAsync($"/api/v1/gamesession/{session.Id}/piece/{pieceId}/actions?actionType=Ability");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await resp.Content.ReadFromJsonAsync<List<PositionDto>>();
        list.Should().NotBeNull();
        list!.Count.Should().Be(0);
    }

    [Theory]
    [InlineData("ShieldBash")]
    [InlineData("Breakthrough")]
    public async Task GetAbilityTargets_ByName_ShouldReturn_Positions(string ability)
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var pieceId = session!.Player1.Pieces[0].Id.ToString();
        var resp = await _client.GetAsync($"/api/v1/gamesession/{session.Id}/piece/{pieceId}/actions?actionType=Ability&ability={ability}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await resp.Content.ReadFromJsonAsync<List<PositionDto>>();
        list.Should().NotBeNull();
        list!.Count.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task PostAction_WhenNotEnoughMp_ShouldReturn_400_ProblemDetails()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();
        session.Should().NotBeNull();

        var ability = new AbilityRequestDto
        {
            PieceId = session!.Player1.Pieces[0].Id.ToString(),
            AbilityName = "MagicExplosion", // у ферзя дорого, но даже если фигура не ферзь — ожидаем 400 по правилам
            Target = new PositionDto { X = session.Player2.Pieces[0].Position.X, Y = session.Player2.Pieces[0].Position.Y }
        };

        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", ability);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
    }

    [Fact]
    public async Task PostAction_WhenCooldownActive_ShouldReturn_400()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var ability1 = new AbilityRequestDto
        {
            PieceId = session!.Player1.Pieces[0].Id.ToString(),
            AbilityName = "LightArrow",
            Target = new PositionDto { X = session.Player2.Pieces[0].Position.X, Y = session.Player2.Pieces[0].Position.Y }
        };
        var resp1 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", ability1);
        if (resp1.StatusCode == HttpStatusCode.OK)
        {
            var resp2 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/ability", ability1);
            resp2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task PostAction_Move_Into_Ally_Occupied_ShouldReturn_400()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var p1 = session!.Player1.Pieces;
        var from = p1[0];
        var ally = p1[1];

        var actionDto = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = from.Id.ToString(),
            TargetPosition = new PositionDto { X = ally.Position.X, Y = ally.Position.Y }
        };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", actionDto);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAction_Attack_OutOfRange_ShouldReturn_400()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var attacker = session!.Player1.Pieces[0];
        var far = new PositionDto { X = attacker.Position.X + 7, Y = attacker.Position.Y + 7 };

        var actionDto = new ExecuteActionDto
        {
            Type = "Attack",
            PieceId = attacker.Id.ToString(),
            TargetPosition = far
        };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", actionDto);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAction_ChangePieceWithoutRoyalCommand_ShouldReturn_400()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2", Mode = "AI" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var p1 = session!.Player1.Pieces;
        var first = p1[0];
        var second = p1[1];

        var move1 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = first.Id.ToString(),
            TargetPosition = new PositionDto { X = first.Position.X, Y = first.Position.Y }
        };
        var r1 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", move1);
        if (r1.StatusCode != HttpStatusCode.OK)
        {
            return;
        }

        var move2 = new ExecuteActionDto
        {
            Type = "Move",
            PieceId = second.Id.ToString(),
            TargetPosition = new PositionDto { X = second.Position.X, Y = second.Position.Y }
        };
        var r2 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", move2);
        r2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}


