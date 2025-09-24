using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public sealed class PieceDefinitionDtoConfiguration : IEntityTypeConfiguration<PieceDefinitionDto>
{
  public void Configure(EntityTypeBuilder<PieceDefinitionDto> builder)
  {
    builder.HasKey(p => p.Id);

    builder.Property(p => p.PieceId)
      .IsRequired()
      .HasMaxLength(50);

    builder.Property(p => p.HP).IsRequired();
    builder.Property(p => p.ATK).IsRequired();
    builder.Property(p => p.Range).IsRequired();
    builder.Property(p => p.Movement).IsRequired();
    builder.Property(p => p.Energy).IsRequired();
    builder.Property(p => p.ExpToEvolve).IsRequired();

    builder.HasIndex(p => p.PieceId);
  }
}


