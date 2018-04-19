using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class AddBeatmapCache : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CachedBeatmaps",
                columns: table => new
                {
                    Bid = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    AR = table.Column<double>(nullable: false),
                    Approved = table.Column<int>(nullable: false),
                    ApprovedDateOffset = table.Column<DateTimeOffset>(nullable: true),
                    Artist = table.Column<string>(nullable: false),
                    Bpm = table.Column<double>(nullable: false),
                    CS = table.Column<double>(nullable: false),
                    Creator = table.Column<string>(nullable: false),
                    DifficultyName = table.Column<string>(nullable: false),
                    FavoriteCount = table.Column<int>(nullable: false),
                    FileMD5 = table.Column<string>(nullable: false),
                    Genre = table.Column<int>(nullable: false),
                    HP = table.Column<double>(nullable: false),
                    HitLength = table.Column<int>(nullable: false),
                    Language = table.Column<int>(nullable: false),
                    LastUpdateOffset = table.Column<DateTimeOffset>(nullable: false),
                    MaxCombo = table.Column<int>(nullable: true),
                    OD = table.Column<double>(nullable: false),
                    Sid = table.Column<int>(nullable: false),
                    Source = table.Column<string>(nullable: false),
                    Stars = table.Column<double>(nullable: false),
                    Tags = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    TotalLength = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CachedBeatmaps", x => new { x.Bid, x.Mode });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CachedBeatmaps");
        }
    }
}
