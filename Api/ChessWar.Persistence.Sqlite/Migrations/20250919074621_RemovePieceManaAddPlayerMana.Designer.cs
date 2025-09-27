using System;
using ChessWar.Persistence.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ChessWar.Persistence.Sqlite.Migrations
{
    [DbContext(typeof(ChessWarDbContext))]
    [Migration("20250919074621_RemovePieceManaAddPlayerMana")]
    partial class RemovePieceManaAddPlayerMana
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.8");

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.BalancePayloadDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("BalanceVersionId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Json")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BalanceVersionId")
                        .IsUnique();

                    b.ToTable("BalancePayloads", (string)null);
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.BalanceVersionDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Comment")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int?>("GlobalsId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("PublishedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("PublishedBy")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("Version")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GlobalsId");

                    b.HasIndex("Status");

                    b.HasIndex("Version")
                        .IsUnique();

                    b.ToTable("BalanceVersions");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.EvolutionRuleDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("BalanceVersionDtoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("From")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("To")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("BalanceVersionDtoId");

                    b.HasIndex("From");

                    b.HasIndex("To");

                    b.ToTable("EvolutionRules");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.GlobalRulesDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CooldownTickPhase")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("MpRegenPerTurn")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("GlobalRules");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.PieceDefinitionDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ATK")
                        .HasColumnType("INTEGER");

                    b.Property<Guid?>("BalanceVersionDtoId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Energy")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExpToEvolve")
                        .HasColumnType("INTEGER");

                    b.Property<int>("HP")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Movement")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PieceId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("Range")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BalanceVersionDtoId");

                    b.HasIndex("PieceId");

                    b.ToTable("PieceDefinitions");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.PieceDto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ATK")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AbilityCooldownsJson")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValue("{}");

                    b.Property<int>("HP")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFirstMove")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(true);

                    b.Property<int>("Movement")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionX")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionY")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Range")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Team")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("XP")
                        .HasColumnType("INTEGER");

                    b.Property<int>("XPToEvolve")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Team");

                    b.HasIndex("Type");

                    b.HasIndex("PositionX", "PositionY")
                        .IsUnique();

                    b.ToTable("Pieces");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.PlayerDto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("MP")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.Property<int>("MaxMP")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("Victories")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.BalancePayloadDto", b =>
                {
                    b.HasOne("ChessWar.Persistence.Core.Entities.BalanceVersionDto", "BalanceVersion")
                        .WithOne()
                        .HasForeignKey("ChessWar.Persistence.Core.Entities.BalancePayloadDto", "BalanceVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BalanceVersion");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.BalanceVersionDto", b =>
                {
                    b.HasOne("ChessWar.Persistence.Core.Entities.GlobalRulesDto", "Globals")
                        .WithMany()
                        .HasForeignKey("GlobalsId");

                    b.Navigation("Globals");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.EvolutionRuleDto", b =>
                {
                    b.HasOne("ChessWar.Persistence.Core.Entities.BalanceVersionDto", null)
                        .WithMany("EvolutionRules")
                        .HasForeignKey("BalanceVersionDtoId");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.PieceDefinitionDto", b =>
                {
                    b.HasOne("ChessWar.Persistence.Core.Entities.BalanceVersionDto", null)
                        .WithMany("Pieces")
                        .HasForeignKey("BalanceVersionDtoId");
                });

            modelBuilder.Entity("ChessWar.Persistence.Core.Entities.BalanceVersionDto", b =>
                {
                    b.Navigation("EvolutionRules");

                    b.Navigation("Pieces");
                });
#pragma warning restore 612, 618
        }
    }
}
