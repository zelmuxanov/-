using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "BirthDate",
                table: "Documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNumber",
                table: "Documents",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssuedBy",
                table: "Documents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Documents",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentNumber",
                table: "Documents",
                column: "DocumentNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Status",
                table: "Documents",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documents_DocumentNumber",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Status",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DocumentNumber",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IssuedBy",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "Pending");
        }
    }
}
