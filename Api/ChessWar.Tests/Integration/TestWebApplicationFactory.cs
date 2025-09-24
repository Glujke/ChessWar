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
            // Remove all existing DbContext registrations
            var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ChessWarDbContext>));
            if (dbContextDescriptor != null)
                services.Remove(dbContextDescriptor);

            // Remove the DbContext itself
            var contextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ChessWarDbContext));
            if (contextDescriptor != null)
                services.Remove(contextDescriptor);

            // Remove all SQLite-related services
            var sqliteServices = services.Where(s => s.ServiceType.FullName?.Contains("Sqlite") == true).ToList();
            foreach (var service in sqliteServices)
            {
                services.Remove(service);
            }

            // Remove existing BalanceConfigProvider
            var configProviderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IBalanceConfigProvider));
            if (configProviderDescriptor != null)
                services.Remove(configProviderDescriptor);

            // Add InMemory database for testing
            services.AddDbContext<ChessWarDbContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });

            // Add test BalanceConfigProvider
            services.AddScoped<IBalanceConfigProvider>(_ => _TestConfig.CreateProvider());
            
            // Don't override Application and Infrastructure services - they're already registered in Program.cs
            // Only add specific test services if needed
        });
        
        // Don't override the app pipeline - it's already configured in Program.cs
        // The TestWebApplicationFactory should only override specific services, not the entire pipeline
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChessWarDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public new async Task DisposeAsync()
    {
        // No cleanup needed for InMemory database
        await base.DisposeAsync();
    }
}
