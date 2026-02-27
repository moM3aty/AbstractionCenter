using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbstractionCenter.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageToRegistrationRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationRequests_AspNetUsers_StudentId",
                table: "RegistrationRequests");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "RegistrationRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "RegistrationRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptFilePath",
                table: "RegistrationRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationRequests_AspNetUsers_StudentId",
                table: "RegistrationRequests",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationRequests_AspNetUsers_StudentId",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "RegistrationRequests");

            migrationBuilder.DropColumn(
                name: "ReceiptFilePath",
                table: "RegistrationRequests");

            migrationBuilder.AlterColumn<string>(
                name: "StudentId",
                table: "RegistrationRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationRequests_AspNetUsers_StudentId",
                table: "RegistrationRequests",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
