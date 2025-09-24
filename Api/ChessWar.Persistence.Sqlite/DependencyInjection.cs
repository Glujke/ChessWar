using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChessWar.Persistence.Sqlite;

public static class DependencyInjection
{
  public static IServiceCollection AddChessWarPersistenceSqlite(this IServiceCollection services, IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("Default")
      ?? "Data Source=App_Data/chesswar.db";

    services.AddDbContext<ChessWarDbContext>(options =>
      options.UseSqlite(connectionString, sqlite =>
        sqlite.MigrationsAssembly(typeof(ChessWarDbContext).Assembly.FullName)));

    return services;
  }
}


