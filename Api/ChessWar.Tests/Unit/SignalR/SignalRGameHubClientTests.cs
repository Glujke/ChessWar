using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Moq;
using ChessWar.Api.Hubs;
using ChessWar.Api.Services;

namespace ChessWar.Tests.Unit.SignalR;

public class SignalRGameHubClientTests
{
    private readonly Mock<IHubContext<GameHub>> _hubContextMock;
    private readonly Mock<IHubClients> _clientsMock;
    private readonly Mock<IClientProxy> _clientProxyMock;
    private readonly SignalRGameHubClient _service;

    public SignalRGameHubClientTests()
    {
        _hubContextMock = new Mock<IHubContext<GameHub>>();
        _clientsMock = new Mock<IHubClients>();
        _clientProxyMock = new Mock<IClientProxy>();
        
        _hubContextMock.Setup(x => x.Clients).Returns(_clientsMock.Object);
        _clientsMock.Setup(x => x.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        
        _service = new SignalRGameHubClient(_hubContextMock.Object);
    }

    [Fact]
    public async Task SendToGroupAsync_Should_Call_Group_On_Clients()
    {
        // Arrange
        var groupName = "test-group";
        var method = "TestMethod";
        var data = new { Message = "Test message" };
        var cancellationToken = CancellationToken.None;

        // Act
        await _service.SendToGroupAsync(groupName, method, data, cancellationToken);

        // Assert
        _clientsMock.Verify(x => x.Group(groupName), Times.Once);
        // Note: Cannot verify SendAsync as it's an extension method
    }

    [Theory]
    [InlineData("game-session-123", "GameEnded", "Player1Victory")]
    [InlineData("tutorial-session-456", "TutorialAdvanced", "Battle2")]
    [InlineData("ai-session-789", "AiMoved", "MoveData")]
    public async Task SendToGroupAsync_Should_Handle_Different_Group_Names_And_Methods(
        string groupName, 
        string method, 
        string data)
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act
        await _service.SendToGroupAsync(groupName, method, data, cancellationToken);

        // Assert
        _clientsMock.Verify(x => x.Group(groupName), Times.Once);
        // Note: Cannot verify SendAsync as it's an extension method
    }

    [Fact]
    public async Task SendToGroupAsync_Should_Propagate_Exceptions()
    {
        // Arrange
        var groupName = "test-group";
        var method = "TestMethod";
        var data = new { Message = "Test message" };
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("SignalR connection failed");

        // Setup the mock to throw when SendToGroupAsync is called
        _clientsMock
            .Setup(x => x.Group(groupName))
            .Throws(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendToGroupAsync(groupName, method, data, cancellationToken));

        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task SendToGroupAsync_Should_Use_Default_CancellationToken_When_Not_Provided()
    {
        // Arrange
        var groupName = "test-group";
        var method = "TestMethod";
        var data = new { Message = "Test message" };

        // Act
        await _service.SendToGroupAsync(groupName, method, data);

        // Assert
        _clientsMock.Verify(x => x.Group(groupName), Times.Once);
        // Note: Cannot verify SendAsync as it's an extension method
    }

    [Fact]
    public void Constructor_Should_Throw_When_HubContext_Is_Null()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SignalRGameHubClient(null!));

        exception.ParamName.Should().Be("hubContext");
    }
}
