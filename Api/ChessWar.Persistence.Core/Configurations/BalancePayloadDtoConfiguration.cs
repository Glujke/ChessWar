using ChessWar.Persistence.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChessWar.Persistence.Core.Configurations;

public sealed class BalancePayloadDtoConfiguration : IEntityTypeConfiguration<BalancePayloadDto>
{
  public void Configure(EntityTypeBuilder<BalancePayloadDto> builder)
  {
    builder.ToTable("BalancePayloads");

    builder.HasKey(p => p.Id);

    builder.Property(p => p.Json)
      .IsRequired();

    builder.HasIndex(p => p.BalanceVersionId)
      .IsUnique();

    builder.HasOne(p => p.BalanceVersion)
      .WithOne()
      .HasForeignKey<BalancePayloadDto>(p => p.BalanceVersionId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}


