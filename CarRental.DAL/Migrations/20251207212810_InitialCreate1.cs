using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Documents",
                newName: "DocumentType");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Documents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "Documents");

            migrationBuilder.RenameColumn(
                name: "DocumentType",
                table: "Documents",
                newName: "Type");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "Documents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }
    }
}
