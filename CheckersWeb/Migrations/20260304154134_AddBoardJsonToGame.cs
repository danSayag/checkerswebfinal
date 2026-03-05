using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckersWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddBoardJsonToGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BoardJson",
                table: "Games",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardJson",
                table: "Games");
        }
    }
}
