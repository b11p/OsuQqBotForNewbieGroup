using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Bleatingsheep.OsuQqBot.Database.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Charts",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ChartCreator = table.Column<long>(nullable: false),
                    ChartDescription = table.Column<string>(nullable: false),
                    ChartName = table.Column<string>(nullable: false),
                    EndTime = table.Column<DateTimeOffset>(nullable: true),
                    IsRunning = table.Column<bool>(nullable: false),
                    MaximumPerformance = table.Column<double>(nullable: true),
                    Public = table.Column<bool>(nullable: false),
                    RecommendPerformance = table.Column<double>(nullable: false),
                    StartTime = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charts", x => x.ChartId);
                });

            migrationBuilder.CreateTable(
                name: "ChartAdministrators",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    Administrator = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartAdministrators", x => new { x.ChartId, x.Administrator });
                    table.ForeignKey(
                        name: "FK_ChartAdministrators_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartMaps",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    BeatmapId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    BannedMods = table.Column<int>(nullable: false),
                    ForceMods = table.Column<int>(nullable: false),
                    RequiredMods = table.Column<int>(nullable: false),
                    ScoreCalculation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartMaps", x => new { x.ChartId, x.BeatmapId, x.Mode });
                    table.ForeignKey(
                        name: "FK_ChartMaps_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChartValidGroups",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    GroupId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartValidGroups", x => new { x.ChartId, x.GroupId });
                    table.ForeignKey(
                        name: "FK_ChartValidGroups_Charts_ChartId",
                        column: x => x.ChartId,
                        principalTable: "Charts",
                        principalColumn: "ChartId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Commits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Accuracy = table.Column<double>(nullable: false),
                    BeatmapId = table.Column<int>(nullable: false),
                    ChartBeatmapBeatmapId = table.Column<int>(nullable: true),
                    ChartBeatmapChartId = table.Column<int>(nullable: true),
                    ChartBeatmapMode = table.Column<int>(nullable: true),
                    ChartId = table.Column<int>(nullable: false),
                    Combo = table.Column<int>(nullable: false),
                    Date = table.Column<DateTimeOffset>(nullable: false),
                    Mods = table.Column<int>(nullable: false),
                    PPWhenCommit = table.Column<double>(nullable: false),
                    Rank = table.Column<string>(nullable: false),
                    Score = table.Column<long>(nullable: false),
                    Uid = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Commits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Commits_ChartMaps_ChartBeatmapChartId_ChartBeatmapBeatmapId_ChartBeatmapMode",
                        columns: x => new { x.ChartBeatmapChartId, x.ChartBeatmapBeatmapId, x.ChartBeatmapMode },
                        principalTable: "ChartMaps",
                        principalColumns: new[] { "ChartId", "BeatmapId", "Mode" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Commits_ChartBeatmapChartId_ChartBeatmapBeatmapId_ChartBeatmapMode",
                table: "Commits",
                columns: new[] { "ChartBeatmapChartId", "ChartBeatmapBeatmapId", "ChartBeatmapMode" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartAdministrators");

            migrationBuilder.DropTable(
                name: "ChartValidGroups");

            migrationBuilder.DropTable(
                name: "Commits");

            migrationBuilder.DropTable(
                name: "ChartMaps");

            migrationBuilder.DropTable(
                name: "Charts");
        }
    }
}
