using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class AddGroupField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BotGroupFields",
                columns: table => new
                {
                    GroupId = table.Column<long>(type: "bigint", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    Data = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BotGroupFields", x => new { x.GroupId, x.FieldName });
                });

            migrationBuilder.CreateIndex(
                name: "IX_BotGroupFields_FieldName",
                table: "BotGroupFields",
                column: "FieldName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BotGroupFields");
        }
    }
}
