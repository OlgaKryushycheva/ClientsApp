// Файл 20260417110000_AddExecutorDismissedFrom.cs створений EF Core для фіксації схеми БД у конкретній міграції.
// Ці класи описують, які SQL-зміни треба застосувати під час оновлення структури таблиць.
﻿using System;
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
