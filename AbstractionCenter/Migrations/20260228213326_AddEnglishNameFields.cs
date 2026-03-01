using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbstractionCenter.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishNameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullNameEn",
                table: "RegistrationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullNameEn",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullNameEn",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "FullNameEn",
                table: "AspNetUsers");
        }
    }
}
