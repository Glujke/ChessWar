using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChessWar.Persistence.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddShieldAndNeighborFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NeighborCount",
                table: "Pieces",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ShieldHp",
                table: "Pieces",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NeighborCount",
                table: "Pieces");

            migrationBuilder.DropColumn(
                name: "ShieldHp",
                table: "Pieces");
        }
    }
}
