using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public class PlayerDtoConfiguration : IEntityTypeConfiguration<PlayerDto>
{
    public void Configure(EntityTypeBuilder<PlayerDto> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Victories)
            .HasDefaultValue(0);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.MP)
            .HasDefaultValue(0);

        builder.Property(p => p.MaxMP)
            .HasDefaultValue(0);
    }
}
