using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChessWar.Persistence.Sqlite;
using ChessWar.Domain.Interfaces.Configuration;
using ChessWar.Tests.Unit;

namespace ChessWar.Tests.Integration;

public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChessWarDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ChessWarDbContext));
            if (contextDescriptor != null)
                services.Remove(contextDescriptor);

            var sqliteServices = services.Where(s => s.ServiceType.FullName?.Contains("Sqlite") == true).ToList();
            foreach (var service in sqliteServices)
            {
                services.Remove(service);
            }

            var configProviderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBalanceConfigProvider));
            if (configProviderDescriptor != null)
                services.Remove(configProviderDescriptor);

            services.AddDbContext<ChessWarDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            services.AddScoped<IBalanceConfigProvider>(_ => _TestConfig.CreateProvider());
            
        });
        
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChessWarDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
    }
}
