using ChessWar.Persistence.Core.Configurations;
using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChessWar.Persistence.Sqlite;

public class ChessWarDbContext : DbContext
{
  public ChessWarDbContext(DbContextOptions<ChessWarDbContext> options) : base(options) {}

  public DbSet<BalanceVersionDto> BalanceVersions => Set<BalanceVersionDto>();
  public DbSet<BalancePayloadDto> BalancePayloads => Set<BalancePayloadDto>();
  public DbSet<PieceDefinitionDto> PieceDefinitions => Set<PieceDefinitionDto>();
  public DbSet<EvolutionRuleDto> EvolutionRules => Set<EvolutionRuleDto>();
  public DbSet<GlobalRulesDto> GlobalRules => Set<GlobalRulesDto>();
  public DbSet<PieceDto> Pieces => Set<PieceDto>();
  public DbSet<PlayerDto> Players => Set<PlayerDto>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfiguration(new BalanceVersionDtoConfiguration());
    modelBuilder.ApplyConfiguration(new BalancePayloadDtoConfiguration());
    modelBuilder.ApplyConfiguration(new PieceDefinitionDtoConfiguration());
    modelBuilder.ApplyConfiguration(new EvolutionRuleDtoConfiguration());
    modelBuilder.ApplyConfiguration(new GlobalRulesDtoConfiguration());
    modelBuilder.ApplyConfiguration(new PieceDtoConfiguration());
    modelBuilder.ApplyConfiguration(new PlayerDtoConfiguration());
  }
}
