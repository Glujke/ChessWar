using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using ChessWar.Persistence.Sqlite;
using ChessWar.Application.Interfaces.Pieces;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Application.Services.Pieces;
using ChessWar.Application.Services.Board;
using ChessWar.Infrastructure;

namespace ChessWar.Tests.Integration;

public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddOpenApi();
        
        services.AddDbContext<ChessWarDbContext>(options =>
        {
            options.UseInMemoryDatabase("TestDatabase");
        });
        
        services.AddScoped<IPieceService, PieceService>();
        services.AddScoped<IBoardService, BoardService>();
        
        services.AddInfrastructure();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
