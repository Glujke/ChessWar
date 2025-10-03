using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Domain.Entities;
using ChessWar.Infrastructure.Services;

namespace ChessWar.Tests.Unit;

public class PieceConfigServiceTests
{
    [Fact]
    public async Task GetActiveAsync_ReturnsNull_WhenDatabaseEmpty()
    {
        var repository = new Mock<IBalanceVersionRepository>();
        repository.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync((BalanceVersion?)null);

        var cache = new MemoryCache(new MemoryCacheOptions());
        IPieceConfigService service = new PieceConfigService(repository.Object, cache);

        var result = await service.GetActiveAsync(CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsCachedVersion_WhenInCache()
    {
        var expectedVersion = new BalanceVersion
        {
            Id = Guid.NewGuid(),
            Version = "v1.0.0",
            Status = "Active",
            Comment = "Test version"
        };

        var repository = new Mock<IBalanceVersionRepository>();
        var cache = new MemoryCache(new MemoryCacheOptions());

        cache.Set("piece-config:active", expectedVersion, TimeSpan.FromMinutes(5));

        IPieceConfigService service = new PieceConfigService(repository.Object, cache);

        var result = await service.GetActiveAsync(CancellationToken.None);

        result.Should().Be(expectedVersion);
        repository.Verify(r => r.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsFromRepository_WhenNotInCache()
    {
        var expectedVersion = new BalanceVersion
        {
            Id = Guid.NewGuid(),
            Version = "v1.0.0",
            Status = "Active",
            Comment = "Test version"
        };

        var repository = new Mock<IBalanceVersionRepository>();
        repository.Setup(r => r.GetActiveAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expectedVersion);

        var cache = new MemoryCache(new MemoryCacheOptions());
        IPieceConfigService service = new PieceConfigService(repository.Object, cache);

        var result = await service.GetActiveAsync(CancellationToken.None);

        result.Should().Be(expectedVersion);
        repository.Verify(r => r.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
