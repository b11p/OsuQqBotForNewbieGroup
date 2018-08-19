using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class CachePlus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeatmapPlusCache",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Stars = table.Column<float>(nullable: false),
                    AimTotal = table.Column<float>(nullable: false),
                    AimJump = table.Column<float>(nullable: false),
                    AimFlow = table.Column<float>(nullable: false),
                    Precision = table.Column<float>(nullable: false),
                    Speed = table.Column<float>(nullable: false),
                    Stamina = table.Column<float>(nullable: false),
                    Accuracy = table.Column<float>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapPlusCache", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatmapPlusCache");
        }
    }
}
