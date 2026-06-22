using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RemoveVINLicensePlatePricePerHour : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "PricePerHour",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "VIN",
                table: "Cars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Cars",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerHour",
                table: "Cars",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VIN",
                table: "Cars",
                type: "character varying(17)",
                maxLength: 17,
                nullable: false,
                defaultValue: "");
        }
    }
}
