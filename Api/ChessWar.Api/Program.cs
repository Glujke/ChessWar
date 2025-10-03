using ChessWar.Persistence.Sqlite;
using Microsoft.EntityFrameworkCore;
using ChessWar.Application.Interfaces.Configuration;
using ChessWar.Application.Interfaces.Board;
using ChessWar.Infrastructure;
using ChessWar.Application;
using ChessWar.Api.Extensions;
using ChessWar.Application.Services.Board;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var loggerConfiguration = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ChessWar.Api")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .MinimumLevel.Information();

if (builder.Environment.IsProduction())
{
    loggerConfiguration = loggerConfiguration.WriteTo.File(
        "Logs/chesswar-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
}

Log.Logger = loggerConfiguration.CreateLogger();

try
{
    Log.Information("Starting ChessWar API application");

    builder.Host.UseSerilog();

    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "ChessWar API",
            Version = "v1",
            Description = "API для игры ChessWar"
        });
    });
    builder.Services.AddControllers();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevCors", policy =>
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
            if (origins.Length > 0)
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            }
            else
            {
                policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => false);
            }
        });
    });
    builder.Services.AddProblemDetails();

    builder.Services.AddSignalR();

    builder.Services.AddAutoMapper(typeof(Program));

    builder.Services.AddChessWarPersistenceSqlite(builder.Configuration);
    builder.Services.AddMemoryCache();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();

    builder.Services.AddHostedService<TurnProcessingBackgroundService>();
    builder.Services.AddSingleton<ITurnProcessingQueue, TurnProcessingBackgroundService>();

    builder.Services.AddScoped<IBalanceConfigValidator, ChessWar.Api.Services.BalanceConfigValidator>();
    builder.Services.AddScoped<ChessWar.Api.Services.IErrorHandlingService, ChessWar.Api.Services.ErrorHandlingService>();
    builder.Services.AddGameModeServices();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseProblemDetails();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChessWar API v1");
            c.RoutePrefix = string.Empty;
        });
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseCors("DevCors");
    }

    app.MapControllers();

    app.MapHub<ChessWar.Api.Hubs.GameHub>("/gameHub");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ChessWarDbContext>();

        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            var dataPath = app.Environment.IsDevelopment() ? "App_Data" : "/app/data";
            System.IO.Directory.CreateDirectory(dataPath);
            db.Database.Migrate();
        }

        if (!await db.BalanceVersions.AnyAsync())
        {
            db.BalanceVersions.Add(new ChessWar.Persistence.Core.Entities.BalanceVersionDto
            {
                Id = Guid.NewGuid(),
                Version = "v1.0.0",
                Status = "Active",
                Comment = "Initial seed",
                CreatedAt = DateTimeOffset.UtcNow,
                PublishedAt = DateTimeOffset.UtcNow,
                PublishedBy = "seed"
            });
            await db.SaveChangesAsync();
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
