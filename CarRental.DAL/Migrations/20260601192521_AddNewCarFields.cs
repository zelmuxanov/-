using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCarFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Deposit",
                table: "Cars",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MileageLimitPerDay",
                table: "Cars",
                type: "integer",
                nullable: false,
                defaultValue: 250);

            migrationBuilder.AddColumn<decimal>(
                name: "OverMileagePricePerKm",
                table: "Cars",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay15",
                table: "Cars",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay30",
                table: "Cars",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Deposit",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "MileageLimitPerDay",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "OverMileagePricePerKm",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "PricePerDay15",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "PricePerDay30",
                table: "Cars");
        }
    }
}
