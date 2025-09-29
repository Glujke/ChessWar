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
        var groupName = "test-group";
        var method = "TestMethod";
        var data = new { Message = "Test message" };
        var cancellationToken = CancellationToken.None;

        await _service.SendToGroupAsync(groupName, method, data, cancellationToken);

        _clientsMock.Verify(x => x.Group(groupName), Times.Once);
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
        var cancellationToken = CancellationToken.None;

        await _service.SendToGroupAsync(groupName, method, data, cancellationToken);

        _clientsMock.Verify(x => x.Group(groupName), Times.Once);
    }

    [Fact]
    public async Task SendToGroupAsync_Should_Propagate_Exceptions()
    {
        var groupName = "test-group";
        var method = "TestMethod";
        var data = new { Message = "Test message" };
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("SignalR connection failed");

        _clientsMock
            .Setup(x => x.Group(groupName))
            .Throws(expectedException);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SendToGroupAsync(groupName, method, data, cancellationToken));

        exception.Should().Be(expectedException);
    }

    [Fact]
    public async Task SendToGroupAsync_Should_Use_Default_CancellationToken_When_Not_Provided()
    {
        var groupName = "test-group";
        var method = "TestMethod";
        var data = new { Message = "Test message" };

        await _service.SendToGroupAsync(groupName, method, data);

        _clientsMock.Verify(x => x.Group(groupName), Times.Once);
    }

    [Fact]
    public void Constructor_Should_Throw_When_HubContext_Is_Null()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new SignalRGameHubClient(null!));

        exception.ParamName.Should().Be("hubContext");
    }
}
