using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class AddPlayRecordQueryTemp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayRecordQueryTemps",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    StartNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayRecordQueryTemps", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayRecordQueryTemps_UserId_Mode",
                table: "PlayRecordQueryTemps",
                columns: new[] { "UserId", "Mode" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayRecordQueryTemps");
        }
    }
}
