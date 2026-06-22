using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserExtendedProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DateOfBirth",
                table: "AspNetUsers",
                newName: "RegistrationDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "BirthDate",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DrivingExperience",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DrivingExperience",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "RegistrationDate",
                table: "AspNetUsers",
                newName: "DateOfBirth");
        }
    }
}
