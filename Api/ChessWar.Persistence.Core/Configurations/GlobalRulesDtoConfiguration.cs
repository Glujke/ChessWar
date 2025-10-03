using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public sealed class GlobalRulesDtoConfiguration : IEntityTypeConfiguration<GlobalRulesDto>
{
    public void Configure(EntityTypeBuilder<GlobalRulesDto> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.MpRegenPerTurn).IsRequired();
        builder.Property(g => g.CooldownTickPhase)
          .IsRequired()
          .HasMaxLength(50);
    }
}


