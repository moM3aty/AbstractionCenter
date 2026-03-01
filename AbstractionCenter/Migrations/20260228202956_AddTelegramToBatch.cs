using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbstractionCenter.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramToBatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TelegramGroupUrl",
                table: "Batches",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TelegramGroupUrl",
                table: "Batches");
        }
    }
}
