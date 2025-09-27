using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessWar.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class RemovePieceManaAddPlayerMana : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MP",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "MaxMP",
                table: "Pieces");

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Victories = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MP = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    MaxMP = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.AddColumn<int>(
                name: "MP",
                table: "Pieces",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxMP",
                table: "Pieces",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
