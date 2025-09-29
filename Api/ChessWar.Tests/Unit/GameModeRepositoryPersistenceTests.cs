using ChessWar.Domain.Entities;
using ChessWar.Domain.Enums;
using ChessWar.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace ChessWar.Tests.Unit;

public class GameModeRepositoryPersistenceTests
{
    [Fact]
    public async Task SaveAndGet_TutorialSession_ShouldPersistInRepository()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var repository = new GameModeRepository(memoryCache);
        var player = new Player("player123", Team.Elves);
        var session = new TutorialSession(player);

        await repository.SaveModeAsync(session);
        var exists = await repository.ModeExistsAsync(session.Id);
        var loaded = await repository.GetModeByIdAsync<TutorialSession>(session.Id);

        Assert.True(exists);
        Assert.NotNull(loaded);
        Assert.Equal(session.Id, loaded!.Id);
        Assert.Equal(GameMode.Tutorial, loaded.Mode);
    }
}


