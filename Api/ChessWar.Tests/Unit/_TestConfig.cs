using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Domain.Interfaces.DataAccess;
using ChessWar.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using ChessWar.Domain.Entities;

namespace ChessWar.Tests.Unit;

internal static class _TestConfig
{
    public static IBalanceConfigProvider CreateProvider()
    {
        var versionRepo = new Mock<IBalanceVersionRepository>();
        var payloadRepo = new Mock<IBalancePayloadRepository>();
        versionRepo.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((BalanceVersion?)null);
        var logger = Mock.Of<ILogger<BalanceConfigProvider>>();
        return new BalanceConfigProvider(versionRepo.Object, payloadRepo.Object, logger);
    }
}


