using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using ChessWar.Application.Services.AI;
using ChessWar.Domain.Interfaces.AI;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.TurnManagement;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.ValueObjects;

namespace ChessWar.Tests.Integration.E2E;

/// <summary>
/// E2E тесты для полного прохождения Tutorial
/// </summary>
public class CompleteTutorialE2ETests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly ILogger<CompleteTutorialE2ETests> _logger;
    private ChessWar.Domain.Services.AI.AIService? _hardAI;

    public CompleteTutorialE2ETests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _logger = factory.Services.GetRequiredService<ILogger<CompleteTutorialE2ETests>>();
    }

    private void InitializeHardAI()
    {
        try
        {
            var probabilityMatrixMock = new Mock<IProbabilityMatrix>();
            var gameStateEvaluatorMock = new Mock<IGameStateEvaluator>();
            var difficultyProviderMock = new Mock<IAIDifficultyLevel>();
            var turnServiceMock = new Mock<ITurnService>();
            var abilityServiceMock = new Mock<IAbilityService>();
            var loggerMock = new Mock<ILogger<ChessWar.Domain.Services.AI.AIService>>();

            difficultyProviderMock.Setup(x => x.GetDifficultyLevel(It.IsAny<ChessWar.Domain.Entities.AI>())).Returns(AIDifficultyLevel.Hard);

            turnServiceMock.Setup(x => x.ExecuteMove(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                .Returns(true);

            turnServiceMock.Setup(x => x.ExecuteAttack(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>(), It.IsAny<Position>()))
                .Returns(true);

            turnServiceMock.Setup(x => x.GetAvailableMoves(It.IsAny<GameSession>(), It.IsAny<Turn>(), It.IsAny<Piece>()))
                .Returns(new List<Position> { new Position(0, 2), new Position(1, 2), new Position(2, 2) });

            turnServiceMock.Setup(x => x.GetAvailableAttacks(It.IsAny<Turn>(), It.IsAny<Piece>()))
                .Returns(new List<Position> { new Position(0, 3), new Position(1, 3) });

            abilityServiceMock.Setup(x => x.UseAbility(It.IsAny<Piece>(), It.IsAny<string>(), It.IsAny<Position>(), It.IsAny<List<Piece>>()))
                .Returns(true);

            gameStateEvaluatorMock.Setup(x => x.EvaluateGameState(It.IsAny<GameSession>(), It.IsAny<Player>()))
                .Returns(0.5);

            probabilityMatrixMock.Setup(x => x.GetActionProbability(It.IsAny<GameSession>(), It.IsAny<GameAction>()))
                .Returns(0.8);

            var actionGenerator = new ChessWar.Domain.Services.AI.ActionGenerator(turnServiceMock.Object, abilityServiceMock.Object, Mock.Of<ILogger<ChessWar.Domain.Services.AI.ActionGenerator>>());
            var actionSelector = new ChessWar.Domain.Services.AI.ActionSelector(probabilityMatrixMock.Object, gameStateEvaluatorMock.Object, difficultyProviderMock.Object);
            var actionExecutor = new ChessWar.Domain.Services.AI.ActionExecutor(turnServiceMock.Object, abilityServiceMock.Object);

            _hardAI = new ChessWar.Domain.Services.AI.AIService(
                actionGenerator,
                actionSelector,
                actionExecutor,
                loggerMock.Object
            );
        }
        catch (Exception ex)
        {
            _logger.LogError($"ОШИБКА ИНИЦИАЛИЗАЦИИ HARD AI: {ex.Message}");
            _hardAI = null;
        }
    }

    private static StringContent Json(object obj) => new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

    private async Task<GameSession?> GetGameSession(string gameId)
    {
        try
        {
            var response = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var players = new List<Player>();
            if (root.TryGetProperty("players", out var playersElement))
            {
                foreach (var playerElement in playersElement.EnumerateArray())
                {
                    players.Add(ParsePlayer(playerElement));
                }
            }

            var currentTurn = root.TryGetProperty("currentTurn", out var turnElement) ? turnElement.GetInt32() : 1;

            var player1 = players.FirstOrDefault(p => p.Name.Contains("Player1") || p.Name.Contains("1"));
            var player2 = players.FirstOrDefault(p => p.Name.Contains("Player2") || p.Name.Contains("2"));

            if (player1 == null || player2 == null)
            {
                return null;
            }

            return new GameSession(player1, player2, "Tutorial");
        }
        catch
        {
            return null;
        }
    }

    private Player ParsePlayer(JsonElement playerElement)
    {
        var name = playerElement.GetProperty("name").GetString() ?? "Unknown";
        var pieces = new List<Piece>();

        if (playerElement.TryGetProperty("pieces", out var piecesElement))
        {
            foreach (var pieceElement in piecesElement.EnumerateArray())
            {
                var piece = new Piece
                {
                    Id = pieceElement.GetProperty("id").GetInt32(),
                    Type = (PieceType)pieceElement.GetProperty("type").GetInt32(),
                    Team = (Team)pieceElement.GetProperty("team").GetInt32(),
                    Position = new Position(
                        pieceElement.GetProperty("position").GetProperty("x").GetInt32(),
                        pieceElement.GetProperty("position").GetProperty("y").GetInt32()
                    )
                };
                pieces.Add(piece);
            }
        }

        return new Player(name, pieces);
    }

    private async Task LogBoard(string gameId, string context)
    {
        try
        {
            var boardResponse = await _client.GetAsync($"/api/v1/gamesession/{gameId}/board");
            if (boardResponse.IsSuccessStatusCode)
            {
                var boardJson = await boardResponse.Content.ReadAsStringAsync();
                _logger.LogInformation($"=== ДОСКА {context} ===");
                _logger.LogInformation(boardJson);
                _logger.LogInformation("=== КОНЕЦ ДОСКИ ===");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Не удалось получить доску {context}: {ex.Message}");
        }
    }

    [Fact]
    public async Task CompleteTutorial_FromStartToFinish_ShouldWork()
    {
        var startResponse = await _client.PostAsync("/api/v1/game/tutorial?embed=(game)", Json(new { playerId = "player-e2e-test" }));
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var startJson = await startResponse.Content.ReadAsStringAsync();
        using var startDoc = JsonDocument.Parse(startJson);
        var gameId = startDoc.RootElement.GetProperty("gameSessionId").GetString();
        gameId.Should().NotBeNullOrEmpty();

        var game = startDoc.RootElement.GetProperty("_embedded").GetProperty("game");
        var player2Pieces = game.GetProperty("player2").GetProperty("pieces").EnumerateArray().ToList();

        var king = player2Pieces.FirstOrDefault(p =>
        {
            var typeElement = p.GetProperty("type");
            return typeElement.ValueKind == JsonValueKind.String ?
                typeElement.GetString() == "King" :
                typeElement.GetInt32() == 5;
        });
        king.Should().NotBeNull();
        king.GetProperty("position").GetProperty("x").GetInt32().Should().Be(4);
        king.GetProperty("position").GetProperty("y").GetInt32().Should().Be(7);

        var pawns = player2Pieces.Where(p =>
        {
            var typeElement = p.GetProperty("type");
            var isPawn = typeElement.ValueKind == JsonValueKind.String ?
                typeElement.GetString() == "Pawn" :
                typeElement.GetInt32() == 0;
            return isPawn && p.GetProperty("position").GetProperty("y").GetInt32() == 6;
        }).Count();
        pawns.Should().BeGreaterOrEqualTo(6);

        try
        {
            _logger.LogInformation("=== ВЫЗЫВАЕМ PlayTutorialBattle ===");
            await PlayTutorialBattle(gameId!, "Battle1");
            _logger.LogInformation("=== PlayTutorialBattle ЗАВЕРШЕН ===");
        }
        catch (Exception ex)
        {
            _logger.LogError($"ОШИБКА В PlayTutorialBattle: {ex.Message}");
            _logger.LogError($"StackTrace: {ex.StackTrace}");
            throw;
        }

        var advanceResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition?embed=(game)",
            Json(new { action = "advance" }));

        if (advanceResponse.StatusCode == HttpStatusCode.OK)
        {
            var advanceJson = await advanceResponse.Content.ReadAsStringAsync();
            using var advanceDoc = JsonDocument.Parse(advanceJson);
            var battle2GameId = advanceDoc.RootElement.GetProperty("gameSessionId").GetString();
            battle2GameId.Should().NotBeNullOrEmpty();
        }
        else
        {
            advanceResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }



    }

    [Fact]
    public async Task TutorialReplay_ShouldCreateNewGameSession()
    {
        var startResponse = await _client.PostAsync("/api/v1/game/tutorial", Json(new { playerId = "player-replay-test" }));
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var startJson = await startResponse.Content.ReadAsStringAsync();
        using var startDoc = JsonDocument.Parse(startJson);
        var originalGameId = startDoc.RootElement.GetProperty("gameSessionId").GetString();

        var replayResponse = await _client.PostAsync($"/api/v1/gamesession/{originalGameId}/tutorial/transition",
            Json(new { action = "replay" }));
        replayResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var replayJson = await replayResponse.Content.ReadAsStringAsync();
        using var replayDoc = JsonDocument.Parse(replayJson);
        var newGameId = replayDoc.RootElement.GetProperty("gameSessionId").GetString();

        newGameId.Should().NotBeNullOrEmpty();
        newGameId.Should().NotBe(originalGameId);

        var newGameResponse = await _client.GetAsync($"/api/v1/gamesession/{newGameId}");
        newGameResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TutorialAdvance_WithoutVictory_ShouldReturn409()
    {
        var startResponse = await _client.PostAsync("/api/v1/game/tutorial", Json(new { playerId = "player-advance-test" }));
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var startJson = await startResponse.Content.ReadAsStringAsync();
        using var startDoc = JsonDocument.Parse(startJson);
        var gameId = startDoc.RootElement.GetProperty("gameSessionId").GetString();

        var advanceResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/tutorial/transition",
            Json(new { action = "advance" }));
        advanceResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var problemJson = await advanceResponse.Content.ReadAsStringAsync();
        using var problemDoc = JsonDocument.Parse(problemJson);
        var title = problemDoc.RootElement.GetProperty("title").GetString();
        title.Should().Be("StageNotCompleted");
    }

    /// <summary>
    /// Играет Tutorial battle: выполняет простые ходы до победы (максимум 300 ходов).
    /// </summary>
    private async Task PlayTutorialBattle(string gameId, string battleName)
    {
        for (int move = 1; move <= 300; move++)
        {
            var gameStatus = await GetGameStatus(gameId);
            if (gameStatus != "Active")
            {
                break;
            }

            await WaitUntilPlayerTurn(gameId, "player-e2e-test", TimeSpan.FromSeconds(3));

            await MakeRealPlayerMove(gameId, move);

            var ensureActionResponse = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
            if (ensureActionResponse.IsSuccessStatusCode)
            {
                var ensureActionJson = await ensureActionResponse.Content.ReadAsStringAsync();
                using var ensureActionDoc = JsonDocument.Parse(ensureActionJson);
                var ensureCurrentTurn = ensureActionDoc.RootElement.GetProperty("currentTurn");
                var ensureActions = ensureCurrentTurn.GetProperty("actions");
                if (ensureActions.GetArrayLength() == 0)
                {
                    var passRequest = new { type = "Pass", pieceId = "0", targetPosition = (object?)null };
                    await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/action", Json(passRequest));

                    var confirmResp = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
                    if (confirmResp.IsSuccessStatusCode)
                    {
                        var confirmJson = await confirmResp.Content.ReadAsStringAsync();
                        using var confirmDoc = JsonDocument.Parse(confirmJson);
                        var confirmTurn = confirmDoc.RootElement.GetProperty("currentTurn");
                        var confirmActions = confirmTurn.GetProperty("actions");
                        if (confirmActions.GetArrayLength() == 0)
                        {
                            await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/action", Json(passRequest));
                        }
                    }
                }
            }

            await WaitUntilTurnHasActions(gameId, TimeSpan.FromMilliseconds(500));

            var ensureResp = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
            if (ensureResp.IsSuccessStatusCode)
            {
                var ensureJson = await ensureResp.Content.ReadAsStringAsync();
                using var ensureDoc = JsonDocument.Parse(ensureJson);
                var ensureTurn = ensureDoc.RootElement.GetProperty("currentTurn");
                var ensureActions = ensureTurn.GetProperty("actions");
                if (ensureActions.GetArrayLength() == 0)
                {
                    var passReq = new { type = "Pass", pieceId = "0", targetPosition = (object?)null };
                    await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/action", Json(passReq));
                    await WaitUntilTurnHasActions(gameId, TimeSpan.FromMilliseconds(250));
                }
            }

            var endTurnResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/end", null);
            if (endTurnResponse.StatusCode != HttpStatusCode.OK)
            {
                var endBody = await endTurnResponse.Content.ReadAsStringAsync();
                var sessionBeforeEnd = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
                if (sessionBeforeEnd.IsSuccessStatusCode)
                {
                    var content = await sessionBeforeEnd.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    var turn = doc.RootElement.GetProperty("currentTurn");
                    var actions = turn.GetProperty("actions");
                    if (actions.GetArrayLength() == 0)
                    {
                        var passReq = new { type = "Pass", pieceId = "0", targetPosition = (object?)null };
                        await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/action", Json(passReq));
                        await WaitUntilTurnHasActions(gameId, TimeSpan.FromMilliseconds(200));
                    }
                }
                await Task.Delay(100);
                var retry = await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/end", null);
                retry.StatusCode.Should().Be(HttpStatusCode.OK, $"Завершение хода игрока в {battleName} должно быть успешным. Предыдущий ответ: {endTurnResponse.StatusCode} {endBody}");
            }

            await WaitUntilPlayerTurn(gameId, "player-e2e-test", TimeSpan.FromSeconds(5));

            var statusAfterPlayer = await GetGameStatus(gameId);
            if (statusAfterPlayer != "Active")
            {
                break;
            }
        }

        var finalStatus = await GetGameStatus(gameId);
        finalStatus.Should().Be("Active", $"Игра {battleName} должна быть активна после 300 ходов");
    }

    /// <summary>
    /// Ожидает пока активным участником станет указанный игрок, либо игра завершится, в пределах таймаута.
    /// </summary>
    private async Task WaitUntilPlayerTurn(string gameId, string expectedName, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            var resp = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
            if (!resp.IsSuccessStatusCode)
            {
                await Task.Delay(50);
                continue;
            }
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var statusEl = root.GetProperty("status");
            var isFinished = (statusEl.ValueKind == JsonValueKind.String ? (statusEl.GetString() ?? "") == "Player1Victory" || (statusEl.GetString() ?? "") == "Player2Victory" : statusEl.GetInt32() == 2 || statusEl.GetInt32() == 3);
            if (isFinished)
            {
                return;
            }
            var currentTurn = root.GetProperty("currentTurn");
            var active = currentTurn.GetProperty("activeParticipant");
            var name = active.GetProperty("name").GetString() ?? string.Empty;
            if (string.Equals(name, expectedName, StringComparison.Ordinal))
            {
                return;
            }
            await Task.Delay(50);
        }
    }

    /// <summary>
    /// Ожидает, пока в текущем ходу появится хотя бы одно действие, либо истечёт таймаут.
    /// </summary>
    private async Task WaitUntilTurnHasActions(string gameId, TimeSpan timeout)
    {
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            var resp = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
            if (!resp.IsSuccessStatusCode)
            {
                await Task.Delay(25);
                continue;
            }
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var statusEl = root.GetProperty("status");
            var isFinished = (statusEl.ValueKind == JsonValueKind.String ? (statusEl.GetString() ?? "") == "Player1Victory" || (statusEl.GetString() ?? "") == "Player2Victory" : statusEl.GetInt32() == 2 || statusEl.GetInt32() == 3);
            if (isFinished)
            {
                return;
            }
            var currentTurn = root.GetProperty("currentTurn");
            var actions = currentTurn.GetProperty("actions");
            if (actions.GetArrayLength() > 0)
            {
                return;
            }
            await Task.Delay(25);
        }
    }

    private async Task MakeSimplePlayerMove(string gameId, int moveNumber)
    {
        var gameResponse = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
        gameResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var gameJson = await gameResponse.Content.ReadAsStringAsync();
        using var gameDoc = JsonDocument.Parse(gameJson);
        var game = gameDoc.RootElement;

        var player1Pieces = game.GetProperty("player1").GetProperty("pieces").EnumerateArray().ToList();
        var pawn = player1Pieces.FirstOrDefault(p =>
        {
            var typeElement = p.GetProperty("type");
            return typeElement.ValueKind == JsonValueKind.String ?
                typeElement.GetString() == "Pawn" :
                typeElement.GetInt32() == 0;
        });

        if (pawn.ValueKind == JsonValueKind.Undefined)
        {
            return;
        }

        var pawnIdElement = pawn.GetProperty("id");
        var pawnId = pawnIdElement.ValueKind == JsonValueKind.String ?
            pawnIdElement.GetString() :
            pawnIdElement.GetInt32().ToString();
        var pawnPosition = pawn.GetProperty("position");
        var pawnX = pawnPosition.GetProperty("x").GetInt32();
        var pawnY = pawnPosition.GetProperty("y").GetInt32();

        var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/move",
            Json(new
            {
                pieceId = pawnId,
                targetPosition = new { x = pawnX, y = pawnY + 1 } 
            }));

        if (moveResponse.StatusCode == HttpStatusCode.OK)
        {
            return;
        }
    }

    private async Task MakeRealPlayerMove(string gameId, int moveNumber)
    {
        try
        {
            var gameResponse = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
            if (!gameResponse.IsSuccessStatusCode)
            {
                return;
            }

            var gameJson = await gameResponse.Content.ReadAsStringAsync();
            using var gameDoc = JsonDocument.Parse(gameJson);
            var game = gameDoc.RootElement;

            var currentTurn = game.GetProperty("currentTurn");
            var activeParticipant = currentTurn.GetProperty("activeParticipant");
            var activeParticipantName = activeParticipant.GetProperty("name").GetString();
            var activeParticipantType = activeParticipant.GetProperty("type").GetString();

            if (activeParticipantType == "AI" || (activeParticipantName != "player-e2e-test" && !activeParticipantName.Contains("Player1")))
            {
                return;
            }

            var player1Pieces = game.GetProperty("player1").GetProperty("pieces").EnumerateArray().ToList();
            var player2Pieces = game.GetProperty("player2").GetProperty("pieces").EnumerateArray().ToList();

            var alivePlayerPieces = player1Pieces.Where(p => p.GetProperty("isAlive").GetBoolean()).ToList();
            var aliveEnemyPieces = player2Pieces.Where(p => p.GetProperty("isAlive").GetBoolean()).ToList();

            var attackMade = await TryAttackEnemy(gameId, alivePlayerPieces, aliveEnemyPieces, moveNumber);
            if (attackMade) return;

            var defenseMade = await TryDefendKing(gameId, alivePlayerPieces, aliveEnemyPieces, moveNumber);
            if (defenseMade) return;

            var advanceMade = await TryAdvanceCenterPawns(gameId, alivePlayerPieces, moveNumber);
            if (advanceMade) return;

            var abilityUsed = await TryUseAbilities(gameId, alivePlayerPieces, aliveEnemyPieces, moveNumber);
            if (abilityUsed) return;

            await TrySimplePawnAdvance(gameId, alivePlayerPieces, moveNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка умного хода игрока на ходу {moveNumber}: {ex.Message}");
        }

       
        var gameResponseAfter = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
        if (gameResponseAfter.IsSuccessStatusCode)
        {
            var gameJsonAfter = await gameResponseAfter.Content.ReadAsStringAsync();
            using var gameDocAfter = JsonDocument.Parse(gameJsonAfter);
            var gameAfter = gameDocAfter.RootElement;
            var currentTurnAfter = gameAfter.GetProperty("currentTurn");
            var actions = currentTurnAfter.GetProperty("actions");

            if (actions.GetArrayLength() == 0)
            {
                var passRequest = new
                {
                    type = "Pass",
                    pieceId = "0",
                    targetPosition = (object)null
                };

                await _client.PostAsync($"/api/v1/gamesession/{gameId}/turn/action", Json(passRequest));
            }
        }
    }

    private async Task<bool> TryAttackEnemy(string gameId, List<JsonElement> playerPieces, List<JsonElement> enemyPieces, int moveNumber)
    {
        foreach (var piece in playerPieces)
        {
            var pieceId = piece.GetProperty("id").GetInt32();
            var pieceType = piece.GetProperty("type").GetInt32();
            var currentPos = piece.GetProperty("position");
            var currentX = currentPos.GetProperty("x").GetInt32();
            var currentY = currentPos.GetProperty("y").GetInt32();

            foreach (var enemy in enemyPieces)
            {
                var enemyPos = enemy.GetProperty("position");
                var enemyX = enemyPos.GetProperty("x").GetInt32();
                var enemyY = enemyPos.GetProperty("y").GetInt32();

                var distance = Math.Max(Math.Abs(enemyX - currentX), Math.Abs(enemyY - currentY));

                var attackRange = pieceType == 0 ? 1 : 1;

                if (distance <= attackRange)
                {
                    var attackRequest = new
                    {
                        pieceId = pieceId.ToString(),
                        targetPosition = new { x = enemyX, y = enemyY }
                    };

                    var attackResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/attack",
                        Json(attackRequest));

                    if (attackResponse.IsSuccessStatusCode)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private async Task<bool> TryDefendKing(string gameId, List<JsonElement> playerPieces, List<JsonElement> enemyPieces, int moveNumber)
    {
        var king = playerPieces.FirstOrDefault(p => p.GetProperty("type").GetInt32() == 5);
        if (king.ValueKind == JsonValueKind.Undefined) return false;

        var kingPos = king.GetProperty("position");
        var kingX = kingPos.GetProperty("x").GetInt32();
        var kingY = kingPos.GetProperty("y").GetInt32();

        var threatsNearKing = enemyPieces.Where(enemy =>
        {
            var enemyPos = enemy.GetProperty("position");
            var enemyX = enemyPos.GetProperty("x").GetInt32();
            var enemyY = enemyPos.GetProperty("y").GetInt32();
            var distance = Math.Max(Math.Abs(enemyX - kingX), Math.Abs(enemyY - kingY));
            return distance <= 2;
        }).ToList();

        if (threatsNearKing.Any())
        {
            var safeX = Math.Max(0, Math.Min(7, kingX + 1));
            var safeY = Math.Max(0, Math.Min(7, kingY + 1));

            var moveRequest = new
            {
                pieceId = king.GetProperty("id").GetInt32().ToString(),
                targetPosition = new { x = safeX, y = safeY }
            };

            var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/move",
                Json(moveRequest));

            if (moveResponse.IsSuccessStatusCode)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> TryAdvanceCenterPawns(string gameId, List<JsonElement> playerPieces, int moveNumber)
    {
        var centerPawns = playerPieces.Where(p =>
        {
            var type = p.GetProperty("type").GetInt32();
            var pos = p.GetProperty("position");
            var x = pos.GetProperty("x").GetInt32();
            return type == 0 && x >= 3 && x <= 5;
        }).ToList();

        foreach (var pawn in centerPawns)
        {
            var pieceId = pawn.GetProperty("id").GetInt32();
            var currentPos = pawn.GetProperty("position");
            var currentX = currentPos.GetProperty("x").GetInt32();
            var currentY = currentPos.GetProperty("y").GetInt32();

            var targetY = currentY + (currentY == 1 ? 2 : 1);
            if (targetY > 7) targetY = 7;

            var moveRequest = new
            {
                pieceId = pieceId.ToString(),
                targetPosition = new { x = currentX, y = targetY }
            };

            var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/move",
                Json(moveRequest));

            if (moveResponse.IsSuccessStatusCode)
            {
                return true;
            }
        }
        return false;
    }

    private async Task<bool> TryUseAbilities(string gameId, List<JsonElement> playerPieces, List<JsonElement> enemyPieces, int moveNumber)
    {
        return false;
    }

    private async Task<bool> TrySimplePawnAdvance(string gameId, List<JsonElement> playerPieces, int moveNumber)
    {
        var pawn = playerPieces.FirstOrDefault(p => p.GetProperty("type").GetInt32() == 0);
        if (pawn.ValueKind == JsonValueKind.Undefined) return false;

        var pieceId = pawn.GetProperty("id").GetInt32();
        var currentPos = pawn.GetProperty("position");
        var currentX = currentPos.GetProperty("x").GetInt32();
        var currentY = currentPos.GetProperty("y").GetInt32();

        var targetY = currentY + 1;
        if (targetY > 7) targetY = 7;

        var moveRequest = new
        {
            pieceId = pieceId.ToString(),
            targetPosition = new { x = currentX, y = targetY }
        };

        var moveResponse = await _client.PostAsync($"/api/v1/gamesession/{gameId}/move",
            Json(moveRequest));

        if (moveResponse.IsSuccessStatusCode)
        {
            return true;
        }
        return false;
    }

    private async Task<string> GetGameStatus(string gameId)
    {
        var response = await _client.GetAsync($"/api/v1/gamesession/{gameId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var statusElement = doc.RootElement.GetProperty("status");

        return statusElement.ValueKind == JsonValueKind.String ?
            statusElement.GetString() ?? "Unknown" :
            ConvertStatusToString(statusElement.GetInt32());
    }

    private string ConvertStatusToString(int status)
    {
        return status switch
        {
            0 => "Waiting",
            1 => "Active",
            2 => "Player1Victory",
            3 => "Player2Victory",
            _ => "Unknown"
        };
    }
}
