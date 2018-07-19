using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class PlusHistoryV2Fix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlusHistories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Performance = table.Column<int>(nullable: false),
                    AimTotal = table.Column<int>(nullable: false),
                    AimJump = table.Column<int>(nullable: false),
                    AimFlow = table.Column<int>(nullable: false),
                    Precision = table.Column<int>(nullable: false),
                    Speed = table.Column<int>(nullable: false),
                    Stamina = table.Column<int>(nullable: false),
                    Accuracy = table.Column<int>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlusHistories", x => new { x.Id, x.Date });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlusHistories");
        }
    }
}
