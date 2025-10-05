using System.Net;
using System.Net.Http.Json;
using ChessWar.Application.DTOs;
using FluentAssertions;

namespace ChessWar.Tests.Integration;

public class EvolutionIntegrationTests : IntegrationTestBase, IClassFixture<TestWebApplicationFactory>
{
    public EvolutionIntegrationTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Pawn_Reaches_LastRank_ShouldAllow_EvolutionEndpoint()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var pawn = state!.Player1.Pieces.First(p => p.Type.ToString() == "Pawn" && p.Position != null && p.Position.X == 0);

        // Move pawn forward a few steps
        for (int i = 0; i < 3; i++)
        {
            var next = new PositionDto { X = pawn.Position!.X, Y = pawn.Position.Y + 1 };
            var moveReq = new ExecuteActionDto { Type = "Move", PieceId = pawn.Id.ToString(), TargetPosition = next };
            var moveResp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", moveReq);
            moveResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var st = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
            var snap = await st.Content.ReadFromJsonAsync<GameSessionDto>();
            pawn = snap!.Player1.Pieces.First(p => p.Id == pawn.Id);
        }

        var attackCount = 0;
        var maxAttacks = 10;
        while (attackCount < maxAttacks)
        {
            var currentState = await GetGameState(session.Id);
            var targetPiece = currentState.Player2.Pieces.FirstOrDefault(p => p.Position != null && p.Position.X == 1 && p.Position.Y == 6);

            if (targetPiece == null || targetPiece.HP <= 0)
            {
                break;
            }

            var attackAction = new ExecuteActionDto { Type = "Attack", PieceId = pawn.Id.ToString(), TargetPosition = new PositionDto { X = 1, Y = 6 } };
            var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);
            
           
            if (attackResponse.StatusCode != HttpStatusCode.OK)
            {
                break;
            }
            
            attackCount++;
        }

       
       
       
       

        var evolveReq = new { pieceId = pawn.Id.ToString(), targetType = "Knight" };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/evolve", evolveReq);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostEvolve_ShouldReturnConsistentSession_WithPosition()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var stateResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var state = await stateResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var pawn = state!.Player1.Pieces.First(p => p.Type.ToString() == "Pawn" && p.Position != null && p.Position.X == 0);
        while ((pawn.Position?.Y ?? 0) < 5)
        {
            var next = new PositionDto { X = pawn.Position!.X, Y = pawn.Position.Y + 1 };
            var moveReq = new ExecuteActionDto { Type = "Move", PieceId = pawn.Id.ToString(), TargetPosition = next };
            var moveResp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session!.Id}/turn/action", moveReq);
            moveResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var st = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
            var snap = await st.Content.ReadFromJsonAsync<GameSessionDto>();
            pawn = snap!.Player1.Pieces.First(p => p.Id == pawn.Id);
        }

        var attackPos = new PositionDto { X = 1, Y = 6 };
        var attackReq = new ExecuteActionDto { Type = "Attack", PieceId = pawn.Id.ToString(), TargetPosition = attackPos };
        var attackResp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackReq);
        attackResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var attackCount = 0;
        var maxAttacks = 10;
        while (attackCount < maxAttacks)
        {
            var currentState = await GetGameState(session.Id);
            var targetPiece = currentState.Player2.Pieces.FirstOrDefault(p => p.Position != null && p.Position.X == 1 && p.Position.Y == 6);

            if (targetPiece == null || targetPiece.HP <= 0)
            {
                break;
            }

            var attackAction = new ExecuteActionDto { Type = "Attack", PieceId = pawn.Id.ToString(), TargetPosition = new PositionDto { X = 1, Y = 6 } };
            var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);
            
           
            if (attackResponse.StatusCode != HttpStatusCode.OK)
            {
                break;
            }
            
            attackCount++;
        }

       
       
       
       

        var finalState = await GetGameState(session.Id);
        var finalPawn = finalState.Player1.Pieces.First(p => p.Id == pawn.Id);
        var prev = finalPawn.Position;

        var evolveReq = new { pieceId = pawn.Id.ToString(), targetType = "Bishop" };
        var resp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/evolve", evolveReq);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<GameSessionDto>();
        body.Should().NotBeNull();
        var evolved = body!.Player1.Pieces.First(p => p.Id == pawn.Id);
        evolved.Type.ToString().Should().BeOneOf("Bishop", "Knight");
        evolved.Position.Should().NotBeNull();
        evolved.Position.X.Should().Be(prev.X);
        evolved.Position.Y.Should().Be(prev.Y);
    }

    [Fact]
    public async Task PostEvolve_ImmediateGet_ShouldReturnSameConsistentPosition()
    {
        var createDto = new CreateGameSessionDto { Player1Name = "P1", Player2Name = "P2" };
        var createResp = await _client.PostAsJsonAsync("/api/v1/gamesession", createDto);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        var session = await createResp.Content.ReadFromJsonAsync<GameSessionDto>();

        var beforeResp = await _client.GetAsync($"/api/v1/gamesession/{session!.Id}");
        var before = await beforeResp.Content.ReadFromJsonAsync<GameSessionDto>();
        var pawn = before!.Player1.Pieces.First(p => p.Type.ToString() == "Pawn" && p.Position != null && p.Position.X == 0);
        while ((pawn.Position?.Y ?? 0) < 5)
        {
            var next = new PositionDto { X = pawn.Position!.X, Y = pawn.Position.Y + 1 };
            var moveReq = new ExecuteActionDto { Type = "Move", PieceId = pawn.Id.ToString(), TargetPosition = next };
            var moveResp = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session!.Id}/turn/action", moveReq);
            moveResp.StatusCode.Should().Be(HttpStatusCode.OK);
            var st = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
            var snap = await st.Content.ReadFromJsonAsync<GameSessionDto>();
            pawn = snap!.Player1.Pieces.First(p => p.Id == pawn.Id);
        }

        var attackPos2 = new PositionDto { X = 1, Y = 6 };
        var attackReq2 = new ExecuteActionDto { Type = "Attack", PieceId = pawn.Id.ToString(), TargetPosition = attackPos2 };
        var attackResp2 = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackReq2);
        attackResp2.StatusCode.Should().Be(HttpStatusCode.OK);

        var attackCount = 0;
        var maxAttacks = 10;
        while (attackCount < maxAttacks)
        {
            var currentState = await GetGameState(session.Id);
            var targetPiece = currentState.Player2.Pieces.FirstOrDefault(p => p.Position != null && p.Position.X == 1 && p.Position.Y == 6);

            if (targetPiece == null || targetPiece.HP <= 0)
            {
                break;
            }

            var attackAction = new ExecuteActionDto { Type = "Attack", PieceId = pawn.Id.ToString(), TargetPosition = attackPos2 };
            var attackResponse = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/turn/action", attackAction);
            
           
            if (attackResponse.StatusCode != HttpStatusCode.OK)
            {
                break;
            }
            
            attackCount++;
        }

       
       
       
       

        var finalState = await GetGameState(session.Id);
        var finalPawn = finalState.Player1.Pieces.First(p => p.Id == pawn.Id);
        var prev = finalPawn.Position;

        var evolveReq = new { pieceId = pawn.Id.ToString(), targetType = "Knight" };
        var post = await _client.PostAsJsonAsync($"/api/v1/gamesession/{session.Id}/evolve", evolveReq);
        post.StatusCode.Should().Be(HttpStatusCode.OK);
        var postBody = await post.Content.ReadFromJsonAsync<GameSessionDto>();
        var postPiece = postBody!.Player1.Pieces.First(p => p.Id == pawn.Id);

        var get = await _client.GetAsync($"/api/v1/gamesession/{session.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = await get.Content.ReadFromJsonAsync<GameSessionDto>();
        var afterPiece = after!.Player1.Pieces.First(p => p.Id == pawn.Id);

        postPiece.Position.X.Should().Be(prev.X);
        postPiece.Position.Y.Should().Be(prev.Y);
        afterPiece.Position.X.Should().Be(prev.X);
        afterPiece.Position.Y.Should().Be(prev.Y);
        afterPiece.Type.Should().Be(postPiece.Type);
    }

    private async Task<GameSessionDto> GetGameState(Guid sessionId)
    {
        var response = await _client.GetAsync($"/api/v1/gamesession/{sessionId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var state = await response.Content.ReadFromJsonAsync<GameSessionDto>();
        state.Should().NotBeNull();
        return state!;
    }
}


