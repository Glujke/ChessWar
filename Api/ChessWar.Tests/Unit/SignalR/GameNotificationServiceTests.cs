using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Services.GameManagement;

namespace ChessWar.Tests.Unit.SignalR;

public class GameNotificationServiceTests
{
    private readonly Mock<IGameHubClient> _hubClientMock;
    private readonly Mock<ILogger<GameNotificationService>> _loggerMock;
    private readonly GameNotificationService _service;

    public GameNotificationServiceTests()
    {
        _hubClientMock = new Mock<IGameHubClient>();
        _loggerMock = new Mock<ILogger<GameNotificationService>>();
        _service = new GameNotificationService(_hubClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task NotifyGameEndedAsync_Should_Send_GameEnded_Event_To_Group()
    {
        var sessionId = Guid.NewGuid();
        var result = "Player1Victory";
        var message = "Player 1 won the game";
        var cancellationToken = CancellationToken.None;

        await _service.NotifyGameEndedAsync(sessionId, result, message, cancellationToken);

        _hubClientMock.Verify(
            x => x.SendToGroupAsync(
                sessionId.ToString(),
                "GameEnded",
                It.IsAny<object>(),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task NotifyGameEndedAsync_Should_Log_Information()
    {
        var sessionId = Guid.NewGuid();
        var result = "Player2Victory";
        var message = "Player 2 won the game";

        await _service.NotifyGameEndedAsync(sessionId, result, message);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Notified clients about game end in session {sessionId} with result {result}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("Player1Victory", "Player 1 won")]
    [InlineData("Player2Victory", "Player 2 won")]
    [InlineData("Draw", "Game ended in a draw")]
    public async Task NotifyGameEndedAsync_Should_Handle_Different_Results(string result, string message)
    {
        var sessionId = Guid.NewGuid();

        await _service.NotifyGameEndedAsync(sessionId, result, message);

        _hubClientMock.Verify(
            x => x.SendToGroupAsync(
                sessionId.ToString(),
                "GameEnded",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NotifyGameEndedAsync_Should_Throw_When_HubClient_Throws()
    {
        var sessionId = Guid.NewGuid();
        var result = "Player1Victory";
        var message = "Player 1 won";
        var expectedException = new InvalidOperationException("Hub connection failed");

        _hubClientMock
            .Setup(x => x.SendToGroupAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _service.NotifyGameEndedAsync(sessionId, result, message));
        
        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task NotifyAiMoveAsync_Should_Send_AiMoved_Event_To_Group()
    {
        var sessionId = Guid.NewGuid();
        var moveData = new { From = "A1", To = "A3", Piece = "Pawn" };
        var cancellationToken = CancellationToken.None;

        await _service.NotifyAiMoveAsync(sessionId, moveData, cancellationToken);

        _hubClientMock.Verify(
            x => x.SendToGroupAsync(
                sessionId.ToString(),
                "AiMoved",
                moveData,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task NotifyErrorAsync_Should_Send_Error_Event_To_Group()
    {
        var sessionId = Guid.NewGuid();
        var error = "Invalid move attempted";
        var cancellationToken = CancellationToken.None;

        await _service.NotifyErrorAsync(sessionId, error, cancellationToken);

        _hubClientMock.Verify(
            x => x.SendToGroupAsync(
                sessionId.ToString(),
                "Error",
                It.IsAny<object>(),
                cancellationToken),
            Times.Once);
    }
}
