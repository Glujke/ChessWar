using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public class PieceDtoConfiguration : IEntityTypeConfiguration<PieceDto>
{
    public void Configure(EntityTypeBuilder<PieceDto> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Type)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.Team)
            .IsRequired()
            .HasConversion<string>();
            
        builder.Property(p => p.PositionX)
            .IsRequired();
            
        builder.Property(p => p.PositionY)
            .IsRequired();
            
        builder.Property(p => p.HP)
            .IsRequired();
            
        builder.Property(p => p.ATK)
            .IsRequired();
            
        builder.Property(p => p.Range)
            .IsRequired();
            
        builder.Property(p => p.Movement)
            .IsRequired();
            
        builder.Property(p => p.XP)
            .IsRequired();
            
        builder.Property(p => p.XPToEvolve)
            .IsRequired();
            
        builder.Property(p => p.IsFirstMove)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(p => p.AbilityCooldownsJson)
            .IsRequired()
            .HasDefaultValue("{}");
            
        builder.HasIndex(p => new { p.PositionX, p.PositionY })
            .IsUnique();
            
        builder.HasIndex(p => p.Team);
        builder.HasIndex(p => p.Type);
    }
}
