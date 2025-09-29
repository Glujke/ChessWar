using ChessWar.Application.Commands.GameActionCommands;
using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Domain.Interfaces.GameLogic;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.ValueObjects;
using ChessWar.Infrastructure.Services;
using ChessWar.Domain.Interfaces.DataAccess;
using Moq;
using Microsoft.Extensions.Logging;

namespace ChessWar.Tests.Unit;

/// <summary>
/// Тесты для проверки бага в EvolutionCommand - запрет принудительной эволюции в недопустимые типы
/// </summary>
public class EvolutionCommandBugTests
{
    private readonly IEvolutionService _evolutionService;
    private readonly IBalanceConfigProvider _configProvider;

    public EvolutionCommandBugTests()
    {
        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        _configProvider = new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);
        _evolutionService = new ChessWar.Domain.Services.GameLogic.EvolutionService(_configProvider);
    }

    [Fact]
    public async Task EvolutionCommand_ShouldNotAllowPawnToEvolveToQueen()
    {
        var gameSession = CreateMockGameSession();
        var pawn = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        pawn.XP = 20; // Достаточно XP для эволюции в коня/слона, но НЕ в ферзя

        var evolutionCommand = new EvolutionCommand(gameSession, pawn, PieceType.Queen, _evolutionService);

        await Assert.ThrowsAsync<InvalidOperationException>(() => evolutionCommand.ExecuteAsync());
    }

    [Fact]
    public async Task EvolutionCommand_ShouldNotAllowPawnToEvolveToRook()
    {
        var gameSession = CreateMockGameSession();
        var pawn = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        pawn.XP = 20; // Достаточно XP для эволюции в коня/слона, но НЕ в ладью

        var evolutionCommand = new EvolutionCommand(gameSession, pawn, PieceType.Rook, _evolutionService);

        await Assert.ThrowsAsync<InvalidOperationException>(() => evolutionCommand.ExecuteAsync());
    }

    [Fact]
    public async Task EvolutionCommand_ShouldAllowPawnToEvolveToKnight()
    {
        var gameSession = CreateMockGameSession();
        var pawn = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        pawn.XP = 20; // Достаточно XP для эволюции в коня

        var evolutionCommand = new EvolutionCommand(gameSession, pawn, PieceType.Knight, _evolutionService);

        var result = await evolutionCommand.ExecuteAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task EvolutionCommand_ShouldAllowPawnToEvolveToBishop()
    {
        var gameSession = CreateMockGameSession();
        var pawn = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        pawn.XP = 20; // Достаточно XP для эволюции в слона

        var evolutionCommand = new EvolutionCommand(gameSession, pawn, PieceType.Bishop, _evolutionService);

        var result = await evolutionCommand.ExecuteAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task EvolutionCommand_ShouldNotAllowKnightToEvolveToQueen()
    {
        var gameSession = CreateMockGameSession();
        var knight = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        knight.Type = PieceType.Knight; // Меняем тип на коня
        knight.XP = 40; // Достаточно XP для эволюции в ладью, но НЕ в ферзя

        var evolutionCommand = new EvolutionCommand(gameSession, knight, PieceType.Queen, _evolutionService);

        await Assert.ThrowsAsync<InvalidOperationException>(() => evolutionCommand.ExecuteAsync());
    }

    [Fact]
    public async Task EvolutionCommand_ShouldAllowKnightToEvolveToRook()
    {
        var gameSession = CreateMockGameSession();
        var knight = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        knight.Type = PieceType.Knight; // Меняем тип на коня
        knight.XP = 40; // Достаточно XP для эволюции в ладью

        var evolutionCommand = new EvolutionCommand(gameSession, knight, PieceType.Rook, _evolutionService);

        var result = await evolutionCommand.ExecuteAsync();

        Assert.True(result);
    }

    [Fact]
    public async Task EvolutionCommand_ShouldAllowRookToEvolveToQueen()
    {
        var gameSession = CreateMockGameSession();
        var rook = gameSession.Player1.Pieces.First(); // Используем фигуру из GameSession
        rook.Type = PieceType.Rook; // Меняем тип на ладью
        rook.XP = 60; // Достаточно XP для эволюции в ферзя

        var evolutionCommand = new EvolutionCommand(gameSession, rook, PieceType.Queen, _evolutionService);

        var result = await evolutionCommand.ExecuteAsync();

        Assert.True(result);
    }

    private GameSession CreateMockGameSession()
    {
        var player1 = new Player("TestPlayer1", Team.Orcs);
        var player2 = new Player("TestPlayer2", Team.Elves);
        var gameSession = new GameSession(player1, player2, "TestGame");
        
        var piece = new Piece(PieceType.Pawn, Team.Orcs, new Position(0, 0));
        piece.Owner = player1;
        player1.Pieces.Add(piece);
        
        return gameSession;
    }
}
