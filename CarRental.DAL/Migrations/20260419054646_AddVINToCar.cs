using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddVINToCar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VIN",
                table: "Cars",
                type: "character varying(17)",
                maxLength: 17,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VIN",
                table: "Cars");
        }
    }
}
