using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AbstractionCenter.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchPricingAndNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalInstructorIds",
                table: "Batches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DiscountPercentage",
                table: "Batches",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionNote",
                table: "Batches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Batches",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "ShowDiscount",
                table: "Batches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowExecutionNote",
                table: "Batches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShowPrice",
                table: "Batches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalInstructorIds",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "ExecutionNote",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "ShowDiscount",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "ShowExecutionNote",
                table: "Batches");

            migrationBuilder.DropColumn(
                name: "ShowPrice",
                table: "Batches");
        }
    }
}
