using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChessWar.Persistence.Sqlite;

public static class DatabaseSeeder
{
  public static async Task SeedDefaultAsync(ChessWarDbContext db)
  {
    var hasAny = await db.BalanceVersions.AnyAsync();
    if (hasAny)
    {
      return;
    }

    var version = new BalanceVersionDto
    {
      Id = Guid.NewGuid(),
      Version = "v1.0.0",
      Status = "Active",
      Comment = "Initial seed",
      CreatedAt = DateTimeOffset.UtcNow,
      PublishedAt = DateTimeOffset.UtcNow,
      PublishedBy = "seed"
    };

    db.BalanceVersions.Add(version);
    await db.SaveChangesAsync();
  }
}


