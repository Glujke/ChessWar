using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ChessWar.Persistence.Sqlite;

public class DesignTimeFactory : IDesignTimeDbContextFactory<ChessWarDbContext>
{
  public ChessWarDbContext CreateDbContext(string[] args)
  {
    var builder = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", optional: true)
      .AddEnvironmentVariables();

    var configuration = builder.Build();
    var connectionString = configuration.GetConnectionString("Default")
      ?? "Data Source=App_Data/chesswar.db";

    var options = new DbContextOptionsBuilder<ChessWarDbContext>()
      .UseSqlite(connectionString)
      .Options;

    return new ChessWarDbContext(options);
  }
}
