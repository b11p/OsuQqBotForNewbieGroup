using System;
using Bleatingsheep.Osu.ApiClient;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    /// <inheritdoc />
    public partial class AllowNullBeatmapInfoCacheAddExpirationDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<BeatmapInfo>(
                name: "BeatmapInfo",
                table: "BeatmapInfoCache",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(BeatmapInfo),
                oldType: "jsonb");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ExpirationDate",
                table: "BeatmapInfoCache",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BeatmapInfoCache_ExpirationDate",
                table: "BeatmapInfoCache",
                column: "ExpirationDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BeatmapInfoCache_ExpirationDate",
                table: "BeatmapInfoCache");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "BeatmapInfoCache");

            migrationBuilder.AlterColumn<BeatmapInfo>(
                name: "BeatmapInfo",
                table: "BeatmapInfoCache",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(BeatmapInfo),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
