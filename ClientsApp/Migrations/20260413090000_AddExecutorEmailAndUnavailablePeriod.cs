using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientsApp.Migrations
{
    public partial class AddExecutorEmailAndUnavailablePeriod : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Executors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnavailableFrom",
                table: "Executors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnavailableTo",
                table: "Executors",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "UnavailableFrom",
                table: "Executors");

            migrationBuilder.DropColumn(
                name: "UnavailableTo",
                table: "Executors");
        }
    }
}
