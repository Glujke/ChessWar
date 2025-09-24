using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessWar.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GlobalRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MpRegenPerTurn = table.Column<int>(type: "INTEGER", nullable: false),
                    CooldownTickPhase = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pieces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Team = table.Column<string>(type: "TEXT", nullable: false),
                    PositionX = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionY = table.Column<int>(type: "INTEGER", nullable: false),
                    HP = table.Column<int>(type: "INTEGER", nullable: false),
                    ATK = table.Column<int>(type: "INTEGER", nullable: false),
                    Range = table.Column<int>(type: "INTEGER", nullable: false),
                    Movement = table.Column<int>(type: "INTEGER", nullable: false),
                    MP = table.Column<int>(type: "INTEGER", nullable: false),
                    MaxMP = table.Column<int>(type: "INTEGER", nullable: false),
                    XP = table.Column<int>(type: "INTEGER", nullable: false),
                    XPToEvolve = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFirstMove = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    AbilityCooldownsJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pieces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BalanceVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Comment = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    PublishedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    GlobalsId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceVersions_GlobalRules_GlobalsId",
                        column: x => x.GlobalsId,
                        principalTable: "GlobalRules",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BalancePayloads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BalanceVersionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalancePayloads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalancePayloads_BalanceVersions_BalanceVersionId",
                        column: x => x.BalanceVersionId,
                        principalTable: "BalanceVersions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvolutionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    From = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    To = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    BalanceVersionDtoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvolutionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvolutionRules_BalanceVersions_BalanceVersionDtoId",
                        column: x => x.BalanceVersionDtoId,
                        principalTable: "BalanceVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PieceDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PieceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    HP = table.Column<int>(type: "INTEGER", nullable: false),
                    ATK = table.Column<int>(type: "INTEGER", nullable: false),
                    Range = table.Column<int>(type: "INTEGER", nullable: false),
                    Movement = table.Column<int>(type: "INTEGER", nullable: false),
                    Energy = table.Column<int>(type: "INTEGER", nullable: false),
                    ExpToEvolve = table.Column<int>(type: "INTEGER", nullable: false),
                    BalanceVersionDtoId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PieceDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PieceDefinitions_BalanceVersions_BalanceVersionDtoId",
                        column: x => x.BalanceVersionDtoId,
                        principalTable: "BalanceVersions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalancePayloads_BalanceVersionId",
                table: "BalancePayloads",
                column: "BalanceVersionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BalanceVersions_GlobalsId",
                table: "BalanceVersions",
                column: "GlobalsId");

            migrationBuilder.CreateIndex(
                name: "IX_BalanceVersions_Status",
                table: "BalanceVersions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BalanceVersions_Version",
                table: "BalanceVersions",
                column: "Version",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EvolutionRules_BalanceVersionDtoId",
                table: "EvolutionRules",
                column: "BalanceVersionDtoId");

            migrationBuilder.CreateIndex(
                name: "IX_EvolutionRules_From",
                table: "EvolutionRules",
                column: "From");

            migrationBuilder.CreateIndex(
                name: "IX_EvolutionRules_To",
                table: "EvolutionRules",
                column: "To");

            migrationBuilder.CreateIndex(
                name: "IX_PieceDefinitions_BalanceVersionDtoId",
                table: "PieceDefinitions",
                column: "BalanceVersionDtoId");

            migrationBuilder.CreateIndex(
                name: "IX_PieceDefinitions_PieceId",
                table: "PieceDefinitions",
                column: "PieceId");

            migrationBuilder.CreateIndex(
                name: "IX_Pieces_PositionX_PositionY",
                table: "Pieces",
                columns: new[] { "PositionX", "PositionY" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pieces_Team",
                table: "Pieces",
                column: "Team");

            migrationBuilder.CreateIndex(
                name: "IX_Pieces_Type",
                table: "Pieces",
                column: "Type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalancePayloads");

            migrationBuilder.DropTable(
                name: "EvolutionRules");

            migrationBuilder.DropTable(
                name: "PieceDefinitions");

            migrationBuilder.DropTable(
                name: "Pieces");

            migrationBuilder.DropTable(
                name: "BalanceVersions");

            migrationBuilder.DropTable(
                name: "GlobalRules");
        }
    }
}
