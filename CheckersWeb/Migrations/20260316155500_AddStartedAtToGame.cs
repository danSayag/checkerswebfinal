using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckersWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddStartedAtToGame : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MoveNumber",
                table: "Moves",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlayedAt",
                table: "Moves",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Games",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MoveNumber",
                table: "Moves");

            migrationBuilder.DropColumn(
                name: "PlayedAt",
                table: "Moves");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Games");
        }
    }
}
