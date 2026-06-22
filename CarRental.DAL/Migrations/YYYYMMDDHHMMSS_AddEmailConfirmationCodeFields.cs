using Microsoft.EntityFrameworkCore.Migrations;

public partial class AddEmailConfirmationCodeFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "EmailConfirmationCode",
            table: "AspNetUsers",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "EmailConfirmationCodeExpiry",
            table: "AspNetUsers",
            type: "timestamp with time zone",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "EmailConfirmationCode",
            table: "AspNetUsers");

        migrationBuilder.DropColumn(
            name: "EmailConfirmationCodeExpiry",
            table: "AspNetUsers");
    }
}