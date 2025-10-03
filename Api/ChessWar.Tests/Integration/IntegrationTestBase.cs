using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChessWar.Persistence.Sqlite;

namespace ChessWar.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory _factory;
    protected readonly HttpClient _client;
    protected readonly IServiceScope _scope;
    protected readonly ChessWarDbContext _context;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _scope = _factory.Services.CreateScope();
        _context = _scope.ServiceProvider.GetRequiredService<ChessWarDbContext>();
    }

    public async Task InitializeAsync()
    {
        var pieces = await _context.Pieces.ToListAsync();
        _context.Pieces.RemoveRange(pieces);
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        var pieces = await _context.Pieces.ToListAsync();
        _context.Pieces.RemoveRange(pieces);
        await _context.SaveChangesAsync();

        await _context.DisposeAsync();
        _scope.Dispose();
        _client.Dispose();
    }
}
