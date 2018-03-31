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
                name: "ChartBeatmaps",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    BeatmapId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    AllowsFail = table.Column<bool>(nullable: false),
                    BannedMods = table.Column<int>(nullable: false),
                    ForceMods = table.Column<int>(nullable: false),
                    RequiredMods = table.Column<int>(nullable: false),
                    ScoreCalculation = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartBeatmaps", x => new { x.ChartId, x.BeatmapId, x.Mode });
                    table.ForeignKey(
                        name: "FK_ChartBeatmaps_Charts_ChartId",
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
                name: "ChartCommits",
                columns: table => new
                {
                    ChartId = table.Column<int>(nullable: false),
                    BeatmapId = table.Column<int>(nullable: false),
                    Mode = table.Column<int>(nullable: false),
                    Date = table.Column<long>(nullable: false),
                    Accuracy = table.Column<double>(nullable: false),
                    Combo = table.Column<int>(nullable: false),
                    Mods = table.Column<int>(nullable: false),
                    PPWhenCommit = table.Column<double>(nullable: false),
                    Rank = table.Column<string>(nullable: false),
                    Score = table.Column<long>(nullable: false),
                    Uid = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChartCommits", x => new { x.ChartId, x.BeatmapId, x.Mode, x.Date });
                    table.ForeignKey(
                        name: "FK_ChartCommits_ChartBeatmaps_ChartId_BeatmapId_Mode",
                        columns: x => new { x.ChartId, x.BeatmapId, x.Mode },
                        principalTable: "ChartBeatmaps",
                        principalColumns: new[] { "ChartId", "BeatmapId", "Mode" },
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChartAdministrators");

            migrationBuilder.DropTable(
                name: "ChartCommits");

            migrationBuilder.DropTable(
                name: "ChartValidGroups");

            migrationBuilder.DropTable(
                name: "ChartBeatmaps");

            migrationBuilder.DropTable(
                name: "Charts");
        }
    }
}
