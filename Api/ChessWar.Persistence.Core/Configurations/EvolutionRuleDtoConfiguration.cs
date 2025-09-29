using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public sealed class EvolutionRuleDtoConfiguration : IEntityTypeConfiguration<EvolutionRuleDto>
{
  public void Configure(EntityTypeBuilder<EvolutionRuleDto> builder)
  {
    builder.HasKey(r => r.Id);

    builder.Property(r => r.From)
      .IsRequired()
      .HasMaxLength(50);

    builder.Property(r => r.To)
      .IsRequired()
      .HasMaxLength(50);

    builder.HasIndex(r => r.From);
    builder.HasIndex(r => r.To);
  }
}


