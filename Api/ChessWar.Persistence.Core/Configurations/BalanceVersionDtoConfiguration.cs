using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public class BalanceVersionDtoConfiguration : IEntityTypeConfiguration<BalanceVersionDto>
{
    public void Configure(EntityTypeBuilder<BalanceVersionDto> builder)
    {
        builder.HasKey(v => v.Id);
        
        builder.Property(v => v.Version)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(v => v.Status)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(v => v.Comment)
            .HasMaxLength(500);
            
        builder.Property(v => v.CreatedAt)
            .IsRequired();
            
        builder.Property(v => v.PublishedAt);
        
        builder.Property(v => v.PublishedBy)
            .HasMaxLength(100);
            
        builder.HasIndex(v => v.Version)
            .IsUnique();
            
        builder.HasIndex(v => v.Status);
    }
}
