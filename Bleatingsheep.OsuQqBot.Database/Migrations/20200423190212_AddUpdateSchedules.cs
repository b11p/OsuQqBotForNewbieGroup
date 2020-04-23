using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class AddUpdateSchedules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UpdateSchedules",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    NextUpdate = table.Column<DateTimeOffset>(nullable: false),
                    ActiveIndex = table.Column<int>(nullable: false),
                    Version = table.Column<DateTime>(rowVersion: true, nullable: true)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateSchedules", x => new { x.UserId, x.Mode });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UpdateSchedules");
        }
    }
}
