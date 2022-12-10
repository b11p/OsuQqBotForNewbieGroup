using System;
using Bleatingsheep.Osu.ApiClient;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddBeatmapInfoCache : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeatmapInfoCache",
                columns: table => new
                {
                    BeatmapId = table.Column<int>(type: "integer", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    CacheDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    BeatmapInfo = table.Column<BeatmapInfo>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeatmapInfoCache", x => new { x.BeatmapId, x.Mode });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeatmapInfoCache");
        }
    }
}
