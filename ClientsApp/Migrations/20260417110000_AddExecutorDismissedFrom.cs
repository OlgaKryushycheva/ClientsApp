using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientsApp.Migrations
{
    public partial class AddExecutorDismissedFrom : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DismissedFrom",
                table: "Executors",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DismissedFrom",
                table: "Executors");
        }
    }
}
